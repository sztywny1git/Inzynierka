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
    [SerializeField] private int treasureRoomExpand = 1;

    [Header("Prop Spawning Settings")]
    [SerializeField]
    private List<RoomPropDataEntry> roomPropDataList = new List<RoomPropDataEntry>();

    [Header("Managers")]
    [SerializeField] private PuzzleManager puzzleManager; 


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
        // Inicjalizuj tylko, jeśli słownik jest null
        if (roomPropDataMap == null)
        {
            roomPropDataMap = new Dictionary<RoomType, RoomPropData>();
            foreach (var entry in roomPropDataList)
            {
                // Dodatkowe zabezpieczenie przed nullami w Inspektorze
                if (entry.propData != null && !roomPropDataMap.ContainsKey(entry.roomType))
                {
                    roomPropDataMap.Add(entry.roomType, entry.propData);
                }
                else if (entry.propData == null)
                {
                    // To pomoże Ci znaleźć, który slot w Inspektorze jest pusty
                    Debug.LogWarning($"RoomPropData dla typu {entry.roomType} jest pusta w Inspektorze. Sprawdź przypisania!");
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
        
        // 4. Ustawienie typów pokoi i spawnowanie (przeniesione z CreateRooms)
        // UWAGA: Musisz upewnić się, że to wszystko co było na końcu CreateRooms() zostało
        // przeniesione, aby zachować logikę.
        
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

        // 2. ✅ WYWOŁANIE POZA PĘTLĄ (Tylko raz!)
        // PuzzleManager ma już listę 'activePuzzles', więc sam wie, gdzie co postawić w całym lochu.
        if (puzzleManager != null)
        {
            Debug.Log("Generator: Wywołanie spawnowania obiektów zagadek (Globalne).");
            puzzleManager.SpawnPuzzleObjects(new BoundsInt(), spawner); 
        }
    }

    private void SpawnPropsInRooms()
    {
        // Upewnij się, że masz referencję do Spawnera/PropSpawnera
        // Zakładam, że masz już pole Spawner spawner;
        
        // Zbiór wszystkich pozycji, na których NIC nie może się pojawić
        HashSet<Vector2Int> allReservedPositions = new HashSet<Vector2Int>(WallPositions);
        // Dodaj pozycje zarezerwowane przez zagadki
        allReservedPositions.UnionWith(spawner.PuzzleManager.ReservedPuzzlePositions); // Zakładam, że Spawner ma referencję do PuzzleManager
        
        // Zbiór wszystkich pozycji podłogi (cała podłoga)
        HashSet<Vector2Int> allFloorPositions = floorPositions; 

        foreach (var room in allRooms)
        {
            if (roomTypes.TryGetValue(room, out RoomType type))
            {
                if (roomPropDataMap.TryGetValue(type, out RoomPropData propData))
                {
                    // Sprawdź, czy pokój nie jest zbyt mały
                    if (room.size.x < 3 || room.size.y < 3) continue;

                    // 1. Wylicz bezpieczne pozycje podłogi w tym pokoju
                    List<Vector2Int> safeFloorPositions = GetSafeFloorPositionsInRoom(room, allReservedPositions, allFloorPositions);

                    // 2. Wylicz bezpieczne pozycje przy ścianach w tym pokoju
                    List<Vector2Int> safeWallPositions = GetSafeWallPositionsInRoom(room, allReservedPositions, WallPositions);
                    
                    // 3. Spawnowanie propów
                    SpawnFloorProps(room, propData, safeFloorPositions);
                    SpawnWallProps(room, propData, safeWallPositions);
                }
            }
        }
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
            var roomFloor = RunRandomWalk(randomWalkParameters, roomCenter);
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
        var currentRoomCenter = roomCenters[Random.Range(0, roomCenters.Count)];
        roomCenters.Remove(currentRoomCenter);

        while (roomCenters.Count > 0)
        {
            Vector2Int closest = FindClosestPointTo(currentRoomCenter, roomCenters);
            roomCenters.Remove(closest);
            HashSet<Vector2Int> newCorridor = CreateCorridor(currentRoomCenter, closest);
            currentRoomCenter = closest;
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
}
