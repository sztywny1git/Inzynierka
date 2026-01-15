using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public enum RoomType
{
    Undefined,
    Start,
    Standard,
    Puzzle,
    Boss
}

[System.Serializable]
public struct RoomPropDataEntry
{
    public RoomType roomType;
    public RoomPropData propData;
}

public class RoomFirstDungeonGenerator : SimpleRandomWalkDungeonGenerator
{
    [SerializeField]
    private int minRoomWidth = 4, minRoomHeight = 4;
    [SerializeField]
    private int dungeonWidth = 20, dungeonHeight = 20;
    [SerializeField]
    [Range(0, 10)]
    private int offset = 1;
    [SerializeField]
    private bool randomWalkRooms = false;

    [Header("Special Room Settings")]
    [SerializeField]
    [Tooltip("Ile pokoi zagadkowych (Puzzle) ma się pojawić — wybierane z bocznych gałęzi.")]
    private int puzzleRoomCount = 2;

    [Header("Room Size Modifiers")]
    [SerializeField] private int bossRoomExpand = 4;
    [SerializeField] private int puzzleRoomExpand = 2;

    [Header("Prop Spawning Settings")]
    [SerializeField]
    private List<RoomPropDataEntry> roomPropDataList = new List<RoomPropDataEntry>();

    [Header("Managers")]
    [SerializeField] private PuzzleManager puzzleManager; 

    [Header("Debug / Wizualizacja")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color fullGraphColor = Color.yellow;
    [SerializeField] private Color mstColor = Color.blue;
    [SerializeField] private Color finalPathColor = Color.red;

    [Header("Cellular Automata (Smoothing)")]
    [SerializeField] private bool useSmoothing = true;
    [SerializeField] [Range(1, 5)] private int smoothingIterations = 1;

    // Listy do przechowywania krawędzi tylko w celu wyświetlenia ich w Scene View
    private List<GraphAlgorithms.Edge> debugAllEdges = new List<GraphAlgorithms.Edge>();
    private List<GraphAlgorithms.Edge> debugMSTEdges = new List<GraphAlgorithms.Edge>();
    private List<GraphAlgorithms.Edge> debugFinalEdges = new List<GraphAlgorithms.Edge>();


    // Add fields to track rooms
    private BoundsInt bossRoom;
    private BoundsInt playerRoom;
    public List<BoundsInt> allRooms;
    public List<BoundsInt> MainPathRooms;
    protected HashSet<Vector2Int> corridorPositions = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> WallPositions = new HashSet<Vector2Int>();
    public TilemapVisualizer TilemapVisualizer => tilemapVisualizer;
    private Dictionary<RoomType, RoomPropData> roomPropDataMap;
    private Dictionary<BoundsInt, RoomType> roomTypes = new Dictionary<BoundsInt, RoomType>();


    private void InitializePropDataMap()
    {
        // ZAWSZE twórz nowy słownik, aby pobrać świeże dane z Inspektora
        roomPropDataMap = new Dictionary<RoomType, RoomPropData>();

        foreach (var entry in roomPropDataList)
        {
            if (entry.propData != null)
            {
                if (roomPropDataMap.ContainsKey(entry.roomType))
                {
                    Debug.LogWarning($"DUPLIKAT w liście RoomPropData for type {entry.roomType}. Nadpisuję poprzednią wartość.");
                    roomPropDataMap[entry.roomType] = entry.propData;
                }
                else
                {
                    roomPropDataMap.Add(entry.roomType, entry.propData);
                }
            }
            else
            {
                Debug.LogWarning($"W liście 'Room Prop Data List' element dla typu {entry.roomType} ma puste pole 'Prop Data'!");
            }
        }
        
        Debug.Log($"Zainicjalizowano mapę propów. Liczba wpisów: {roomPropDataMap.Count}");
    }

    private void FindWalls()
    {
        WallPositions.Clear();
        foreach (var pos in floorPositions)
        {
            foreach (var dir in Direction2D.eightDirectionList)
            {
                Vector2Int neighbor = pos + dir;
                // Jeśli sąsiad podłogi nie jest podłogą, to jest to ściana
                if (!floorPositions.Contains(neighbor))
                {
                    WallPositions.Add(neighbor);
                }
            }
        }
    }

    protected override void RunProceduralGeneration()
    {
        InitializePropDataMap();
        // Zapewnij, że 'spawner' ma referencję do PuzzleManager
        if (spawner != null && puzzleManager != null)
        {
            // Jeśli używasz metody Initialize z Kroku 1:
            spawner.Initialize(puzzleManager);
        }
        // 1. Wywołaj CreateRooms, która teraz zwraca całą podłogę
        HashSet<Vector2Int> currentFloor = CreateRooms();
        
        // 2. Zapisz zestaw podłogi do pola klasy
        floorPositions = currentFloor;

        // 3. Wizualizacja i generowanie ścian (przeniesione z CreateRooms)
        tilemapVisualizer.Clear();
        tilemapVisualizer.PaintFloorTiles(floorPositions);
        WallGenerator.CreateWalls(floorPositions, tilemapVisualizer);
        
        FindWalls();
        
        // Find and mark the boss room and player room
        FindSpecialRooms(allRooms, floorPositions); // Używamy floorPositions!

        // Assign types to rooms (Start, Boss, Puzzle, Standard)
        AssignRoomTypes(allRooms, floorPositions); // Używamy floorPositions!

        tilemapVisualizer.PaintRoomTypes(roomTypes);

        // Spawn content in rooms
        SpawnRoomContent();
        SpawnPropsInRooms();
        // Spawn player and boss
        SpawnPlayerAndBoss();
    }

    private HashSet<Vector2Int> CreateRooms() 
    {
        allRooms = ProceduralGenerationAlgorithms.BinarySpacePartitioning(new BoundsInt((Vector3Int)startPosition,
            new Vector3Int(dungeonWidth, dungeonHeight, 0)), minRoomWidth, minRoomHeight);

        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();

        ApplyRoomShapeModifications();

        if (randomWalkRooms)
        {
            floor = CreateRoomsRandomly(allRooms);
        }
        else
        {
            floor = CreateSimpleRooms(allRooms);
        }
        List<Vector2Int> roomCenters = FindCentersOfRooms(allRooms);

        HashSet<Vector2Int> corridors = ConnectRooms(roomCenters);

        corridorPositions = corridors;

        floor.UnionWith(corridors);

        return floor;
    }
    private void FindSpecialRooms(List<BoundsInt> roomsList, HashSet<Vector2Int> floor)
    {
        if (roomsList == null || roomsList.Count == 0) return;

        // Find player room (closest to startPosition)
        playerRoom = FindClosestRoomToPosition(roomsList, startPosition);

        // Find boss room (farthest from player room)
        bossRoom = FindFarthestRoomFromPlayer(roomsList, floor, playerRoom);

        Debug.Log($"Player room center: {DungeonHelper.GetRoomCenter(playerRoom)}");
        Debug.Log($"Boss room center: {DungeonHelper.GetRoomCenter(bossRoom)}");
    }

    private BoundsInt FindClosestRoomToPosition(List<BoundsInt> roomsList, Vector2Int position)
    {
        BoundsInt closestRoom = roomsList[0];
        float closestDistance = Vector2.Distance(position, DungeonHelper.GetRoomCenter(closestRoom));

        foreach (var room in roomsList)
        {
            float distance = Vector2.Distance(position, DungeonHelper.GetRoomCenter(room));
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestRoom = room;
            }
        }

        return closestRoom;
    }

    private BoundsInt FindFarthestRoomFromPlayer(List<BoundsInt> roomsList, HashSet<Vector2Int> floor, BoundsInt playerRoom)
    {
        // Create a graph from the floor tiles
        Dictionary<Vector2Int, List<Vector2Int>> graph = CreateGraph(floor);

        // Use BFS to find the farthest point from the player room
        Vector2Int startPoint = DungeonHelper.GetRoomCenter(playerRoom);
        Vector2Int farthestPoint = FindFarthestPointBFS(graph, startPoint);

        // Find which room contains the farthest point
        return FindRoomContainingPoint(roomsList, farthestPoint);
    }

    private void SpawnPlayerAndBoss()
    {
        if (spawner != null)
        {
            // Spawn player in the center of the closest room
            Vector2Int playerSpawnPosition = DungeonHelper.GetRoomCenter(playerRoom);
            spawner.SpawnPlayer(playerSpawnPosition);

            // Spawn boss in the center of the boss room
            Vector2Int bossSpawnPosition = DungeonHelper.GetRoomCenter(bossRoom);
            spawner.SpawnBoss(bossSpawnPosition);
        }
        else
        {
            Debug.LogWarning("Spawner reference not set in RoomFirstDungeonGenerator");
        }
    }

    private void SpawnRoomContent()
    {
        // 1. Pętla dla elementów specyficznych dla danego pokoju (wrogowie, dekoracje)
        foreach (var kvp in roomTypes)
        {
            BoundsInt room = kvp.Key;
            RoomType type = kvp.Value;
            
            // Vector2Int center = DungeonHelper.GetRoomCenter(room); // Opcjonalne, jeśli używane

            switch (type)
            {
                case RoomType.Puzzle:
                    break;

                case RoomType.Standard:
                    // Tu w przyszłości dodasz spawnowanie wrogów
                    // spawner.SpawnEnemies(room, floor, 3, 5);
                    break;

                case RoomType.Start:
                    // spawner.SpawnSavePoint(center);
                    break;
            }
        }

        // PuzzleManager ma już listę 'activePuzzles', więc sam wie, gdzie co postawić w całym lochu.
        if (puzzleManager != null)
        {
            Debug.Log("Generator: Wywołanie spawnowania obiektów zagadek (Globalne).");
            puzzleManager.SpawnPuzzleObjects(new BoundsInt(), spawner); 
        }
    }

    private void SpawnPropsInRooms()
    {
        Debug.Log($"[DIAGNOSTYKA] Start SpawnPropsInRooms. Liczba ścian (WallPositions): {WallPositions.Count}");

        if (WallPositions.Count == 0)
        {
            Debug.LogError("[BŁĄD KRYTYCZNY] WallPositions jest puste! WFC nie postawi żadnych pochodni/skrzyń, bo nie widzi ścian. Sprawdź czy FindWalls() jest wywoływane.");
        }

        PropWFC wfcSolver = new PropWFC();
        
        // Budowanie globalnych rezerwacji
        HashSet<Vector2Int> globalReserved = new HashSet<Vector2Int>();
        if (spawner.PuzzleManager != null)
        {
            globalReserved.UnionWith(spawner.PuzzleManager.ReservedPuzzlePositions);
        }

        int totalPropsSpawned = 0;

        foreach (var room in allRooms)
        {
            if (room.size.x < 4 || room.size.y < 4) continue;

            if (roomTypes.TryGetValue(room, out RoomType type))
            {
                if (!roomPropDataMap.ContainsKey(type))
                {
                    // Debug.LogWarning($"Pominięto pokój {type} - brak danych.");
                    continue;
                }

                RoomPropData data = roomPropDataMap[type];

                Dictionary<PropWFC.PropType, int> currentRoomWeights = data.GetWfcWeights();


                // Lokalne rezerwacje
                HashSet<Vector2Int> localReserved = new HashSet<Vector2Int>(globalReserved);
                localReserved.UnionWith(corridorPositions);
                
                // Dodajemy margines korytarzy
                foreach(var corrPos in corridorPositions)
                    foreach(var dir in Direction2D.eightDirectionList)
                        localReserved.Add(corrPos + dir);

                // Rezerwacja środka pokoju
                if (MainPathRooms != null && MainPathRooms.Contains(room))
                {
                    Vector2Int center = DungeonHelper.GetRoomCenter(room);
                    int freeRadius = 2; // UWAGA: Dla małych pokoi (np. 4x4) to zablokuje CAŁY pokój!
                    for (int x = center.x - freeRadius; x <= center.x + freeRadius; x++)
                        for (int y = center.y - freeRadius; y <= center.y + freeRadius; y++)
                            localReserved.Add(new Vector2Int(x, y));
                }

                // Uruchomienie WFC
                var wfcResult = wfcSolver.Run(room, floorPositions, WallPositions, localReserved, currentRoomWeights);

                // Analiza wyników dla tego pokoju
                int propsInThisRoom = 0;
                Dictionary<PropWFC.PropType, int> counts = new Dictionary<PropWFC.PropType, int>();
                
                foreach (var kvp in wfcResult)
                {
                    // Zliczamy co WFC wygenerowało (statystyka)
                    if (!counts.ContainsKey(kvp.Value)) counts[kvp.Value] = 0;
                    counts[kvp.Value]++;

                    if (!localReserved.Contains(kvp.Key) && kvp.Value != PropWFC.PropType.Empty)
                    {
                        spawner.SpawnWfcProp(kvp.Key, kvp.Value);
                        propsInThisRoom++;
                    }
                }
                
                totalPropsSpawned += propsInThisRoom;

                // Logujemy tylko jeśli pokój jest pusty mimo danych, żeby nie spamować
                if (propsInThisRoom == 0)
                {
                    string stats = string.Join(", ", counts.Select(x => $"{x.Key}: {x.Value}"));
                    Debug.Log($"[POKÓJ {type}] WFC Wynik: {stats}. Zespawnwano: 0. (Może wszystko jest Empty lub Zarezerwowane?)");
                }
            }
        }
        Debug.Log($"[KONIEC] Łącznie zespawnowano propsów: {totalPropsSpawned}");
    }
    private bool IsNearCorridor(Vector2Int pos)
    {
        foreach(var dir in Direction2D.eightDirectionList)
        {
            if (corridorPositions.Contains(pos + dir)) return true;
        }
        return false;
    }

    private void SpawnFloorProps(BoundsInt room, RoomPropData propData, List<Vector2Int> safePositions)
    {
        // Oblicz docelową liczbę propów na podstawie gęstości i rozmiaru pokoju
        int roomArea = room.size.x * room.size.y;
        int targetCount = Mathf.RoundToInt((float)roomArea / 100f * propData.floorPropDensity);
        
        // Ogranicz do dostępnych pozycji
        targetCount = Mathf.Min(targetCount, safePositions.Count);

        // Losuj pozycje i spawnowanie
        for (int i = 0; i < targetCount; i++)
        {
            // Losowanie pozycji
            int index = Random.Range(0, safePositions.Count);
            Vector2Int pos = safePositions[index];
            safePositions.RemoveAt(index);

            // Losowanie propa (używamy struktury PropEntry!)
            RoomPropData.PropEntry propEntry = propData.floorProps[Random.Range(0, propData.floorProps.Count)];
            
            // Wywołanie spawnowania
            spawner.SpawnProp(propEntry, pos);
        }
    }

    private void SpawnWallProps(BoundsInt room, RoomPropData propData, List<Vector2Int> safePositions)
    {
        // Logika podobna do SpawnFloorProps, ale używa WallPropDensity
        // ...
    }
    private List<Vector2Int> GetSafeFloorPositionsInRoom(BoundsInt room, HashSet<Vector2Int> reserved, HashSet<Vector2Int> floor)
    {
        List<Vector2Int> safePositions = new List<Vector2Int>();
        
        // --- UTWORZENIE NOWEJ STREFY WYKLUCZENIA ---
        HashSet<Vector2Int> exclusionZone = new HashSet<Vector2Int>();
        
        // 1. BEZWZGLĘDNE WYKLUCZENIE WSZYSTKICH KORYTARZY
        if (corridorPositions != null)
        {
            // Dodaj wszystkie pozycje korytarzy do czarnej listy
            exclusionZone.UnionWith(corridorPositions);
            
            // Dodaj margines 1 kafelka wokół korytarzy (aby propy nie stały tuż przy korytarzu)
            foreach (var pos in corridorPositions)
            {
                // Pamiętaj: Direction2D.eightDirectionList musi być dostępne!
                foreach (var direction in Direction2D.eightDirectionList) 
                {
                    exclusionZone.Add(pos + direction);
                }
            }
        }
        
        // 2. WYKLUCZENIE GŁÓWNEJ PRZESTRZENI POKOJU (WOKÓŁ ŚCIEŻKI)
        if (MainPathRooms != null && MainPathRooms.Contains(room))
        {
            List<BoundsInt> path = MainPathRooms;
            Vector2Int center = DungeonHelper.GetRoomCenter(room);
            
            // Zdefiniuj szerszą strefę wolną od propów w centrum pokoju
            int freeRadius = 3; // Strefa 7x7 wokół centrum
            
            for (int x = center.x - freeRadius; x <= center.x + freeRadius; x++)
            {
                for (int y = center.y - freeRadius; y <= center.y + freeRadius; y++)
                {
                    exclusionZone.Add(new Vector2Int(x, y));
                }
            }
        }
        // --- KONIEC DEFINICJI STREFY WYKLUCZENIA ---

        // Iterujemy po obszarze BoundsInt
        for (int x = room.xMin + offset; x < room.xMax - offset; x++)
        {
            for (int y = room.yMin + offset; y < room.yMax - offset; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                
                // Warunek: KAŻDA pozycja musi spełnić wszystkie trzy punkty
                if (floor.Contains(pos) && !reserved.Contains(pos) && !exclusionZone.Contains(pos))
                {
                    safePositions.Add(pos);
                }
            }
        }
        return safePositions;
    }

    private List<Vector2Int> GetSafeWallPositionsInRoom(BoundsInt room, HashSet<Vector2Int> reserved, HashSet<Vector2Int> wall)
    {
        List<Vector2Int> safeWallPositions = new List<Vector2Int>();
        
        // Iterujemy po obszarze, który może zawierać ściany (zewnętrzna granica)
        // Zwykle ściany mają szerokość 1, więc wystarczy krawędź.
        for (int x = room.xMin - 1; x <= room.xMax + 1; x++)
        {
            for (int y = room.yMin - 1; y <= room.yMax + 1; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                // Pozycja jest bezpieczna, jeśli:
                // 1. Jest faktycznie kafelkiem ściany (w zestawie WallPositions)
                // 2. Nie jest zarezerwowana przez zagadki (np. brama)
                if (wall.Contains(pos) && !reserved.Contains(pos))
                {
                    safeWallPositions.Add(pos);
                }
            }
        }
        return safeWallPositions;
    }

    private void ScatterObstaclesInRoom(BoundsInt room, HashSet<Vector2Int> floor, int count)
    {
        // Znajdź wszystkie potencjalne miejsca na podłodze w tym pokoju, 
        // które nie są korytarzem (opcjonalnie) i są oddalone od krawędzi.
        var availablePositions = floor
            .Where(p => 
                p.x > room.xMin + offset && p.x < room.xMax - offset &&
                p.y > room.yMin + offset && p.y < room.yMax - offset)
            .ToList();

        for (int i = 0; i < count; i++)
        {
            if (availablePositions.Count == 0) break;

            // Wybierz losową pozycję z dostępnych
            int index = Random.Range(0, availablePositions.Count);
            Vector2Int spawnPos = availablePositions[index];
            availablePositions.RemoveAt(index);

            // spawner.SpawnObstacle(spawnPos); // wymaga nowego kodu w spawnerze
        }
    }

    // Graph and BFS methods
    private Dictionary<Vector2Int, List<Vector2Int>> CreateGraph(HashSet<Vector2Int> floor)
    {
        Dictionary<Vector2Int, List<Vector2Int>> graph = new Dictionary<Vector2Int, List<Vector2Int>>();

        Vector2Int[] directions = {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        foreach (Vector2Int position in floor)
        {
            List<Vector2Int> neighbors = new List<Vector2Int>();

            foreach (Vector2Int direction in directions)
            {
                Vector2Int neighbor = position + direction;
                if (floor.Contains(neighbor))
                {
                    neighbors.Add(neighbor);
                }
            }

            graph[position] = neighbors;
        }

        return graph;
    }

    private Vector2Int FindFarthestPointBFS(Dictionary<Vector2Int, List<Vector2Int>> graph, Vector2Int start)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, int> distances = new Dictionary<Vector2Int, int>();

        queue.Enqueue(start);
        distances[start] = 0;

        Vector2Int farthestPoint = start;
        int maxDistance = 0;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (graph.ContainsKey(current))
            {
                foreach (Vector2Int neighbor in graph[current])
                {
                    if (!distances.ContainsKey(neighbor))
                    {
                        distances[neighbor] = distances[current] + 1;
                        queue.Enqueue(neighbor);

                        if (distances[neighbor] > maxDistance)
                        {
                            maxDistance = distances[neighbor];
                            farthestPoint = neighbor;
                        }
                    }
                }
            }
        }

        return farthestPoint;
    }

    public BoundsInt FindRoomContainingPoint(List<BoundsInt> roomsList, Vector2Int point)
    {
        foreach (BoundsInt room in roomsList)
        {
            // Note: BoundsInt.xMax/yMax are exclusive in some Unity versions; keeping inclusive checks as before.
            if (point.x >= room.xMin && point.x <= room.xMax &&
                point.y >= room.yMin && point.y <= room.yMax)
            {
                return room;
            }
        }

        return roomsList[0];
    }

    // Rest of your existing methods with corrections
    private static List<Vector2Int> FindCentersOfRooms(List<BoundsInt> roomsList)
    {
        List<Vector2Int> roomCenters = new List<Vector2Int>();
        foreach (var room in roomsList)
        {
            Vector3 center3 = room.center;
            roomCenters.Add(new Vector2Int(Mathf.RoundToInt(center3.x), Mathf.RoundToInt(center3.y)));
        }
        return roomCenters;
    }

    private HashSet<Vector2Int> CreateRoomsRandomly(List<BoundsInt> roomsList)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        
        for (int i = 0; i < roomsList.Count; i++)
        {
            var roomBounds = roomsList[i];
            var roomCenter = DungeonHelper.GetRoomCenter(roomBounds);
            
            // 1. Generowanie kształtu (Random Walk)
            var roomFloor = RunRandomWalk(randomWalkParameters, roomCenter);

            // 2. Wygładzanie (opcjonalne, jeśli używasz)
            if (useSmoothing) // Zakładam, że dodałeś to pole w poprzednich krokach
            {
                roomFloor = ProceduralGenerationAlgorithms.CellularAutomataSmoothing(roomFloor, smoothingIterations);
            }

            // =========================================================
            // 3. NOWOŚĆ: Usuwanie izolowanych fragmentów (FILTRACJA)
            // =========================================================
            roomFloor = ProceduralGenerationAlgorithms.RemoveDisconnectedIslands(roomFloor);
            // =========================================================

            // 4. Dodawanie przefiltrowanego pokoju do głównej mapy
            foreach (var position in roomFloor)
            {
                if (position.x >= (roomBounds.xMin + offset) && position.x <= (roomBounds.xMax - offset) &&
                    position.y >= (roomBounds.yMin + offset) && position.y <= (roomBounds.yMax - offset))
                {
                    floor.Add(position);
                }
            }
        }
        return floor;
    }

    private HashSet<Vector2Int> ConnectRooms(List<Vector2Int> roomCenters)
    {
        HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();
        
        // 1. Czyścimy listy debugowania (żeby nie rysować starych linii)
        debugAllEdges.Clear();
        debugMSTEdges.Clear();
        debugFinalEdges.Clear(); // <--- Ważne: czyścimy też czerwoną listę

        // 2. Generowanie WSZYSTKICH krawędzi (Graf Pełny)
        HashSet<GraphAlgorithms.Edge> allEdgesSet = new HashSet<GraphAlgorithms.Edge>();
        for (int i = 0; i < roomCenters.Count; i++)
        {
            for (int j = i + 1; j < roomCenters.Count; j++)
            {
                var edge = new GraphAlgorithms.Edge(roomCenters[i], roomCenters[j]);
                allEdgesSet.Add(edge);
            }
        }
        List<GraphAlgorithms.Edge> allEdgesList = allEdgesSet.ToList();
        
        // ---> ZAPIS DO DEBUGOWANIA (ŻÓŁTE LINIE)
        debugAllEdges = new List<GraphAlgorithms.Edge>(allEdgesList); 

        // 3. Wyznaczanie MST (Minimalne Drzewo Rozpinające - NIEBIESKIE LINIE)
        List<GraphAlgorithms.Edge> mstEdges = GraphAlgorithms.KruskalMST(roomCenters, allEdgesList);
        
        // ---> ZAPIS DO DEBUGOWANIA (NIEBIESKIE LINIE)
        debugMSTEdges = new List<GraphAlgorithms.Edge>(mstEdges);

        // 4. Braiding (Dodawanie pętli - CZERWONE LINIE)
        // Tworzymy nową listę 'finalEdges', która zaczyna się jako kopia MST
        List<GraphAlgorithms.Edge> finalEdges = new List<GraphAlgorithms.Edge>(mstEdges);
        
        // Używamy HashSet do szybkiego sprawdzania, co już jest w MST
        HashSet<GraphAlgorithms.Edge> existingEdgesSet = new HashSet<GraphAlgorithms.Edge>(mstEdges);
        
        // Znajdź krawędzie, których nie ma w MST
        var unusedEdges = allEdgesList.Where(e => !existingEdgesSet.Contains(e)).ToList();
        
        foreach (var edge in unusedEdges)
        {
            // Dodajemy pętlę tylko jeśli krawędź jest krótka (bliscy sąsiedzi) i los sprzyja (5%)
            if (UnityEngine.Random.value < 0.50f && edge.Distance < 25f) 
            {
                finalEdges.Add(edge);
            }
        }

        // ---> ZAPIS DO DEBUGOWANIA (CZERWONE LINIE - Ostateczna ścieżka)
        // To tutaj był Twój błąd - teraz zmienna 'finalEdges' istnieje
        debugFinalEdges = new List<GraphAlgorithms.Edge>(finalEdges);

        // 5. Fizyczne tworzenie korytarzy na mapie
        foreach (var edge in finalEdges)
        {
            HashSet<Vector2Int> newCorridor = CreateCorridor(edge.U, edge.V);
            corridors.UnionWith(newCorridor);
        }

        return corridors;
    }

    private HashSet<Vector2Int> CreateCorridor(Vector2Int currentRoomCenter, Vector2Int destination)
    {
        HashSet<Vector2Int> corridor = new HashSet<Vector2Int>();
        var position = currentRoomCenter;
        corridor.Add(position);
        while (position.y != destination.y)
        {
            if (destination.y > position.y)
            {
                position += Vector2Int.up;
            }
            else if (destination.y < position.y)
            {
                position += Vector2Int.down;
            }
            corridor.Add(position);
        }
        while (position.x != destination.x)
        {
            if (destination.x > position.x)
            {
                position += Vector2Int.right;
            }
            else if (destination.x < position.x)
            {
                position += Vector2Int.left;
            }
            corridor.Add(position);
        }
        return corridor;
    }

    private Vector2Int FindClosestPointTo(Vector2Int currentRoomCenter, List<Vector2Int> roomCenters)
    {
        Vector2Int closest = Vector2Int.zero;
        float distance = float.MaxValue;
        foreach (var position in roomCenters)
        {
            float currentDistance = Vector2.Distance(position, currentRoomCenter);
            if (currentDistance < distance)
            {
                distance = currentDistance;
                closest = position;
            }
        }
        return closest;
    }

    private HashSet<Vector2Int> CreateSimpleRooms(List<BoundsInt> roomsList)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        foreach (var room in roomsList)
        {
            RoomType type = roomTypes.ContainsKey(room)
                ? roomTypes[room]
                : RoomType.Standard;

            HashSet<Vector2Int> layout;

            switch (type)
            {
                case RoomType.Boss:
                    layout = CreateBossArena(room);
                    break;

                case RoomType.Puzzle:
                    layout = CreatePuzzleRoomLayout(room);
                    break;

                default:
                    layout = CreateStandardRoomLayout(room);
                    break;
            }

            floor.UnionWith(layout);
        }

        return floor;
    }

    // ---------- Assign room types ----------
    private void AssignRoomTypes(List<BoundsInt> roomsList, HashSet<Vector2Int> floor)
    {
        roomTypes.Clear();

        if (roomsList == null || roomsList.Count == 0) return;

        // 1. Start Room
        roomTypes[playerRoom] = RoomType.Start;

        // 2. Boss Room
        roomTypes[bossRoom] = RoomType.Boss;

        // 3. Znajdź główną ścieżkę (Start -> Boss)
        var graph = CreateGraph(floor);
        var startCenter = DungeonHelper.GetRoomCenter(playerRoom);
        var bossCenter = DungeonHelper.GetRoomCenter(bossRoom);

        // mainPathVectors to lista Vector2Int reprezentująca ścieżkę korytarzami
        List<Vector2Int> mainPathVectors = ReconstructPathBFS(graph, startCenter, bossCenter); 

        // 4. Konwersja: które pomieszczenia są na głównej ścieżce?
        List<BoundsInt> mainRooms = new List<BoundsInt>();
        HashSet<BoundsInt> mainRoomsSet = new HashSet<BoundsInt>();
        
        foreach (var pos in mainPathVectors)
        {
            var r = FindRoomContainingPoint(roomsList, pos);
            // Upewnij się, że r jest ważnym pokojem (a nie pustym BoundsInt)
            if (r.size.magnitude > 0 && !mainRoomsSet.Contains(r))
            {
                mainRoomsSet.Add(r);
                mainRooms.Add(r); 
            }
        }
        
        // Ustawienie właściwości publicznej dla PuzzleManagera
        MainPathRooms = mainRooms; 


        // 5. Poboczne pomieszczenia
        var sideRooms = roomsList
            .Where(r => r != playerRoom && r != bossRoom && !mainRoomsSet.Contains(r))
            .ToList();

        // Dodanie pokoi z mainPath do puli losowania, jeśli jest za mało bocznych.
        if (sideRooms.Count < puzzleRoomCount)
        {
            var extraRooms = mainRooms
                .Where(r => r != playerRoom && r != bossRoom)
                .ToList();

            sideRooms.AddRange(extraRooms);
        }
        
        // 6. Pomieszczenia z zagadką (losowy wybór)
        var chosenPuzzles = sideRooms
            .OrderBy(x => Random.value)
            .Take(puzzleRoomCount)
            .ToList();

        List<(BoundsInt room, bool isKey)> puzzleRoomsWithContext = new List<(BoundsInt room, bool isKey)>();

        foreach (var pr in chosenPuzzles)
        {
            roomTypes[pr] = RoomType.Puzzle;
            
            // Określamy kluczowość na podstawie tego, czy pokój jest na głównej ścieżce
            bool isKeyPuzzle = mainRoomsSet.Contains(pr); 
            
            puzzleRoomsWithContext.Add((pr, isKeyPuzzle));
        }

        // === ZMODYFIKOWANE WYWOŁANIE: Przekazanie pokoi z kontekstem do menedżera ===
        if (puzzleManager != null)
        {
            Debug.Log($"AssignRoomTypes: Znaleziono {puzzleRoomsWithContext.Count} pokoi do zagadek. Przygotowuję dane.");
            puzzleManager.DungeonGenerator = this; 
            puzzleManager.PreparePuzzles(puzzleRoomsWithContext); // Tutaj zmienna jest dostępna
        }
        else
        {
            Debug.LogError("PuzzleManager jest NULL w Generatorze. Nie można przygotować zagadek.");
        }

        // 7. Pozostałe pokoje = standardowe
        foreach (var room in roomsList)
        {
            if (!roomTypes.ContainsKey(room))
                roomTypes[room] = RoomType.Standard;
        }

        Debug.Log("Room types assigned:");
        foreach (var kvp in roomTypes)
        {
            Debug.Log($"Room center {DungeonHelper.GetRoomCenter(kvp.Key)} -> {kvp.Value}");
        }
    }

    // Reconstruct BFS path from start -> goal (returns list of positions)
    private List<Vector2Int> ReconstructPathBFS(
        Dictionary<Vector2Int, List<Vector2Int>> graph,
        Vector2Int start,
        Vector2Int goal)
    {
        List<Vector2Int> path = new List<Vector2Int>();

        if (start == goal)
        {
            path.Add(start);
            return path;
        }

        if (graph == null || !graph.ContainsKey(start) || !graph.ContainsKey(goal))
        {
            // Try to proceed anyway — if keys missing, attempt a best-effort search
            // Return path containing start and goal so mainRooms detection still works.
            path.Add(start);
            path.Add(goal);
            return path;
        }

        Dictionary<Vector2Int, Vector2Int> parent = new Dictionary<Vector2Int, Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        queue.Enqueue(start);
        visited.Add(start);

        bool found = false;
        while (queue.Count > 0)
        {
            var curr = queue.Dequeue();
            if (curr == goal)
            {
                found = true;
                break;
            }

            if (!graph.ContainsKey(curr)) continue;

            foreach (var n in graph[curr])
            {
                if (!visited.Contains(n))
                {
                    visited.Add(n);
                    parent[n] = curr;
                    queue.Enqueue(n);
                }
            }
        }

        if (!found)
        {
            // fallback: return start and goal so main path detection still works
            path.Add(start);
            path.Add(goal);
            return path;
        }

        var p = goal;
        while (!p.Equals(start))
        {
            path.Add(p);
            if (!parent.ContainsKey(p)) break;
            p = parent[p];
        }

        path.Add(start);
        path.Reverse();
        return path;
    }

    // Expose room type query (helper) - useful later for visualisation/spawning
    public RoomType GetRoomType(BoundsInt room)
    {
        if (roomTypes.ContainsKey(room)) return roomTypes[room];
        return RoomType.Undefined;
    }

    // modyfikacja pokoi specjalnych
    private void ApplyRoomShapeModifications()
    {
        for (int i = 0; i < allRooms.Count; i++)
        {
            var room = allRooms[i];

            if (!roomTypes.ContainsKey(room))
                continue;

            RoomType type = roomTypes[room];

            switch (type)
            {
                case RoomType.Boss:
                    allRooms[i] = ExpandRoom(room, bossRoomExpand);
                    break;

                case RoomType.Puzzle:
                    allRooms[i] = ExpandRoom(room, puzzleRoomExpand);
                    break;

                case RoomType.Standard:
                    // Możesz dodać inne reguły
                    break;
            }
        }
    }

    private BoundsInt ExpandRoom(BoundsInt room, int amount)
    {
        Vector3Int min = room.min - new Vector3Int(amount, amount, 0);
        Vector3Int max = room.max + new Vector3Int(amount, amount, 0);
        Vector3Int size = max - min;
        return new BoundsInt(min, size);
    }

    private HashSet<Vector2Int> CreateBossArena(BoundsInt room)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();

        Vector2Int center = DungeonHelper.GetRoomCenter(room);
        int radius = Mathf.Min(room.size.x, room.size.y) / 2 - 1;

        for (int x = -radius; x <= radius; x++)
            for (int y = -radius; y <= radius; y++)
                if (x * x + y * y <= radius * radius) // okrąg
                    floor.Add(center + new Vector2Int(x, y));

        return floor;
    }

    private HashSet<Vector2Int> CreatePuzzleRoomLayout(BoundsInt room)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();

        Vector2Int center = DungeonHelper.GetRoomCenter(room);

        int width = Mathf.Min(room.size.x, 10);   // stałe pole gry
        int height = Mathf.Min(room.size.y, 10);

        int hx = width / 2;
        int hy = height / 2;

        for (int x = -hx; x <= hx; x++)
            for (int y = -hy; y <= hy; y++)
                floor.Add(center + new Vector2Int(x, y));

        return floor;
    }

    private HashSet<Vector2Int> CreateTreasureRoom(BoundsInt room)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        Vector2Int center = DungeonHelper.GetRoomCenter(room);

        for (int x = -2; x <= 2; x++)
            for (int y = -2; y <= 2; y++)
                floor.Add(center + new Vector2Int(x, y));

        return floor;
    }

    private HashSet<Vector2Int> CreateStandardRoomLayout(BoundsInt room)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        for (int x = room.xMin + offset; x < room.xMax - offset; x++)
            for (int y = room.yMin + offset; y < room.yMax - offset; y++)
                floor.Add(new Vector2Int(x, y));

        return floor;
    }

    private void OpenGate(BoundsInt room, Vector2Int gatePosition)
    {
        // Ta metoda musiałaby faktycznie usunąć obiekt Bramy z gry
        // i ewentualnie zamalować jej płytki na podłogę.
        Debug.Log($"Brama w pokoju {DungeonHelper.GetRoomCenter(room)} otwarta na pozycji {gatePosition}!");
        // spawner.RemoveGate(gatePosition);
    }
    public bool CheckPuzzlePlate(Vector2Int position)
    {
        if (puzzleManager != null)
        {
            return puzzleManager.CheckPressurePlate(position);
        }
        return false;
    }

    public Vector2Int GetCorridorExitPoint(BoundsInt currentRoom, List<BoundsInt> mainPath)
    {
        // 1. Walidacja
        if (mainPath == null || mainPath.Count == 0)
        {
            Debug.LogError("MainPath jest pusty! Nie można znaleźć wyjścia.");
            return Vector2Int.zero;
        }

        int currentIndex = mainPath.IndexOf(currentRoom);

        // Jeśli pokój nie jest na ścieżce lub jest ostatni (Boss), nie ma wyjścia "dalej"
        if (currentIndex == -1 || currentIndex >= mainPath.Count - 1)
        {
            // Debug.LogWarning($"Pokój {currentRoom.center} to koniec ścieżki lub pokój poboczny.");
            return Vector2Int.zero;
        }

        // 2. Cel: Następny pokój na liście
        BoundsInt nextRoom = mainPath[currentIndex + 1];

        // 3. Znajdź połączenie skanując krawędzie
        return FindExitByScanningPerimeter(currentRoom, nextRoom);
    }

    private Vector2Int FindExitByScanningPerimeter(BoundsInt room, BoundsInt targetRoom)
    {
        Vector2 targetCenter = new Vector2(targetRoom.center.x, targetRoom.center.y);
        Vector2Int bestExit = Vector2Int.zero;
        float minDistance = float.MaxValue;

        // Musimy sprawdzić kafelki, które są na samej granicy pokoju LUB tuż za nią.
        // Zakładamy, że floorPositions zawiera już korytarze.
        
        // Lista kierunków do sprawdzania sąsiadów
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        // Skanujemy każdy kafelek wewnątrz pokoju...
        for (int x = room.xMin; x < room.xMax; x++)
        {
            for (int y = room.yMin; y < room.yMax; y++)
            {
                Vector2Int currentPos = new Vector2Int(x, y);

                // ...ale interesują nas tylko te, które są podłogą...
                if (!floorPositions.Contains(currentPos)) continue;

                // ...i mają sąsiada, który JEST podłogą, ale NIE NALEŻY do tego pokoju.
                // To definicja "wejścia do korytarza".

                foreach (var dir in directions)
                {
                    Vector2Int neighbor = currentPos + dir;

                    // Sprawdzamy czy sąsiad to podłoga (czyli korytarz)
                    if (floorPositions.Contains(neighbor))
                    {
                        // Kluczowy warunek: Sąsiad musi być POZA granicami obecnego pokoju
                        if (!room.Contains(new Vector3Int(neighbor.x, neighbor.y, 0)))
                        {
                            // To jest wyjście! Sprawdźmy, czy prowadzi w dobrą stronę.
                            float dist = Vector2.Distance(neighbor, targetCenter);
                            
                            // Dodatkowy priorytet: Wybieramy to wyjście, które jest najbliżej celu
                            if (dist < minDistance)
                            {
                                minDistance = dist;
                                bestExit = neighbor; // Stawiamy bramę na pierwszym kafelku korytarza
                                
                                // Debug wizualny (tylko w Scene View)
                                Debug.DrawLine(new Vector3(currentPos.x, currentPos.y, 0), new Vector3(neighbor.x, neighbor.y, 0), Color.green, 10f);
                            }
                        }
                    }
                }
            }
        }

        if (bestExit == Vector2Int.zero)
        {
            Debug.LogError($"CRITICAL: Nie znaleziono wyjścia z pokoju {room.center} do {targetRoom.center}. Sprawdź generowanie korytarzy!");
            
            // --- FALLBACK (Ostatnia deska ratunku) ---
            // Jeśli skanowanie zawiodło (np. korytarz styka się rogami), weź po prostu
            // kafelek podłogi z pokoju A, który jest najbliżej środka pokoju B.
            // To może postawić drzwi wewnątrz pokoju, ale przynajmniej nie wywali błędu.
            
            float fallbackDistance = float.MaxValue;
            foreach (var pos in floorPositions)
            {
                if (room.Contains(new Vector3Int(pos.x, pos.y, 0)))
                {
                    float d = Vector2.Distance(pos, targetCenter);
                    if (d < fallbackDistance)
                    {
                        fallbackDistance = d;
                        bestExit = pos;
                    }
                }
            }
            Debug.LogWarning($"Użyto Fallback (drzwi wewnątrz pokoju) na pozycji: {bestExit}");
        }

        return bestExit;
    }

    // Znajduje punkt wejścia do pokoju (wyjście z poprzedniego pokoju)
    public Vector2Int GetCorridorEntrancePoint(BoundsInt currentRoom, List<BoundsInt> path)
    {
        int currentIndex = path.IndexOf(currentRoom);

        if (currentIndex <= 0) // Jeśli to Start Room lub pokój nie na ścieżce
        {
            return DungeonHelper.GetRoomCenter(currentRoom); 
        }
        
        // Zamiast nextRoom, bierzemy previousRoom
        BoundsInt previousRoom = path[currentIndex - 1]; 
        
        // Używamy GetCorridorExitPoint, ale odwracamy kolejność pokoi, 
        // aby uzyskać punkt na krawędzi previousRoom najbliższy currentRoom.
        // Lepszym rozwiązaniem jest po prostu użycie GetCorridorExitPoint,
        // jeśli twoja logika korytarzy jest symetryczna.
        
        // Jeśli nie chcesz pisać od nowa logiki, możesz po prostu wywołać GetCorridorExitPoint
        // dla previousRoom. W kontekście naszego generatora, korytarz jest symetryczny.
        return GetCorridorExitPoint(previousRoom, path); // Zwróci pozycję wyjścia z poprzedniego pokoju
    }

    // Sprawdza, czy dany punkt znajduje się wewnątrz jakiegokolwiek zdefiniowanego pokoju.
    private bool IsPointInAnyRoom(Vector2Int point, List<BoundsInt> rooms)
    {
        Vector3Int checkPoint = new Vector3Int(point.x, point.y, 0);

        foreach (var room in rooms)
        {
            // Sprawdź, czy punkt leży wewnątrz pokoju (włącznie z granicą min, wyłączając max).
            // To jest kluczowe! W zależności od tego, jak generujesz pokoje,
            // czasami trzeba dodać korektę.
            
            // Najbezpieczniejsza forma:
            if (checkPoint.x >= room.xMin && checkPoint.x < room.xMax &&
                checkPoint.y >= room.yMin && checkPoint.y < room.yMax)
            {
                // Punkt znajduje się wewnątrz zdefiniowanego obszaru pokoju.
                return true;
            }
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // DIAGNOSTYKA
        if (debugMSTEdges == null || debugMSTEdges.Count == 0)
        {
            // Opcjonalnie: Rysuj tylko, gdy jesteśmy w Play Mode
            if (Application.isPlaying) Debug.Log("Czekam na wygenerowanie lochu...");
            return; 
        }

        // 1. Rysowanie wszystkich możliwych połączeń (Graf Pełny)
        // Rysujemy cienkie, żółte linie - to są krawędzie, które algorytm ROZWAŻAŁ
        if (debugAllEdges != null && debugAllEdges.Count > 0)
        {
            Gizmos.color = fullGraphColor; 
            foreach (var edge in debugAllEdges)
            {
                // Dodajemy mały offset Z, żeby linie były nad podłogą
                Vector3 start = new Vector3(edge.U.x + 0.5f, edge.U.y + 0.5f, -1);
                Vector3 end = new Vector3(edge.V.x + 0.5f, edge.V.y + 0.5f, -1);
                Gizmos.DrawLine(start, end);
            }
        }

        // 2. Rysowanie MST (Minimalne Drzewo Rozpinające)
        // Rysujemy grubsze, niebieskie linie - to krawędzie WYBRANE przez algorytm Kruskala
        if (debugMSTEdges != null && debugMSTEdges.Count > 0)
        {
            Gizmos.color = mstColor;
            foreach (var edge in debugMSTEdges)
            {
                Vector3 start = new Vector3(edge.U.x + 0.5f, edge.U.y + 0.5f, -2); // Wyżej niż żółte
                Vector3 end = new Vector3(edge.V.x + 0.5f, edge.V.y + 0.5f, -2);
                
                // Unity Gizmos nie ma grubości linii, więc rysujemy 3 linie obok siebie dla efektu
                Gizmos.DrawLine(start, end);
                Gizmos.DrawLine(start + Vector3.right * 0.1f, end + Vector3.right * 0.1f);
                Gizmos.DrawLine(start + Vector3.up * 0.1f, end + Vector3.up * 0.1f);
            }
        }

        if (debugFinalEdges != null && debugFinalEdges.Count > 0)
        {
            Gizmos.color = finalPathColor; // Upewnij się, że w Inspektorze to np. Czerwony
            foreach (var edge in debugFinalEdges)
            {
                // Rysujemy jeszcze wyżej (Z = -3), żeby przykryć niebieskie linie
                Vector3 start = new Vector3(edge.U.x + 0.5f, edge.U.y + 0.5f, -3); 
                Vector3 end = new Vector3(edge.V.x + 0.5f, edge.V.y + 0.5f, -3);
                
                // Rysujemy linię
                Gizmos.DrawLine(start, end);
                
                // Opcjonalnie: Rysujemy krzyżyk na środku krawędzi, żeby widzieć gdzie są pętle
                Vector3 center = (start + end) / 2;
                Gizmos.DrawWireSphere(center, 0.2f);
            }
        }
    }
}
