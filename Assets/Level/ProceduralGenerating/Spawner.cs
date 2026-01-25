using UnityEngine;
using System.Collections.Generic;

public class Spawner : MonoBehaviour
{
    // === Istniejące pola ===
    [SerializeField]
    private GameObject playerPrefab;
    [SerializeField]
    private GameObject bossPrefab;

    // === NOWE POLA DLA ZAGADEK ===
    [Header("Puzzle Objects")]
    [SerializeField]
    [Tooltip("Prefab płytki ciśnieniowej.")]
    private GameObject pressurePlatePrefab; 
    
    [SerializeField]
    [Tooltip("Prefab bramy blokującej przejście.")]
    private GameObject gatePrefab; 

    [SerializeField]
    [Tooltip("Prefab skrzyni ze skarbem (nagroda opcjonalna).")]
    private GameObject treasureChestPrefab;

    [SerializeField]
    [Tooltip("Prefab kosoli do zagadki z binarnymi switchami.")]
    private GameObject binaryConsolePrefab;

    
    [SerializeField]
    private float entityZPosition = -1f; 

    [SerializeField] // DodanieSerializeField, jeśli chcesz ustawiać to w Unity Inspector
    private PuzzleManager puzzleManager;

    // Dodanie publicznej właściwości, aby była dostępna z RoomFirstDungeonGenerator
    public PuzzleManager PuzzleManager => puzzleManager;
    [SerializeField] private RoomFirstDungeonGenerator dungeonGenerator; 
    public RoomFirstDungeonGenerator DungeonGenerator { get => dungeonGenerator; set => dungeonGenerator = value; }

    [System.Serializable]
    public struct WfcPropMapping
    {
        public PropWFC.PropType type;
        public GameObject prefab;
        public float offsetRange; // Np. 0.2f dla lekkiego losowego przesunięcia
    }

    [Header("WFC Props Configuration")]
    [SerializeField] private List<WfcPropMapping> wfcPropMappings;
    private Dictionary<PropWFC.PropType, WfcPropMapping> _propLookup;

    // Możesz również stworzyć publiczną metodę do inicjalizacji:
    public void Initialize(PuzzleManager manager)
    {
        puzzleManager = manager;
    }
    private GameObject currentPlayer;
    private GameObject currentBoss;
    
    // NOWA MAPA do śledzenia obiektów zagadek, aby móc je usunąć
    private Dictionary<Vector2Int, GameObject> puzzleObjects = new Dictionary<Vector2Int, GameObject>();

    private void Awake()    
    {
        Debug.unityLogger.logEnabled = false;
        // Jeśli słownik jest pusty na starcie (a zazwyczaj jest po naciśnięciu Play),
        // musimy go odbudować na podstawie obiektów, które już istnieją na scenie.
        if (puzzleObjects.Count == 0)
        {
            RebuildObjectDictionary();
        }
    }

    private void RebuildObjectDictionary()
    {
        //Debug.Log("Spawner: Odbudowywanie słownika obiektów (Rebuild)...");

        // 1. Znajdź wszystkie BRAMY będące dziećmi tego Spawnera
        GateComponent[] gates = GetComponentsInChildren<GateComponent>(true); // true = szukaj też w nieaktywnych
        foreach (var gate in gates)
        {
            RegisterObjectInDictionary(gate.gameObject);
        }

        // 2. Znajdź wszystkie PŁYTKI
        PressurePlateComponent[] plates = GetComponentsInChildren<PressurePlateComponent>(true);
        foreach (var plate in plates)
        {
            RegisterObjectInDictionary(plate.gameObject);
        }
        
        // 3. Znajdź wszystkie SKRZYNIE (jeśli mają komponent, np. TreasureChestComponent lub po prostu tag/nazwę)
        // Jeśli skrzynie nie mają unikalnego komponentu, to trudniej je znaleźć, 
        // ale zazwyczaj błąd dotyczy Bram, więc to jest priorytet.
        
        // Opcjonalnie: Znajdź konsole
        BinaryConsoleComponent[] consoles = GetComponentsInChildren<BinaryConsoleComponent>(true);
        foreach (var console in consoles)
        {
            RegisterObjectInDictionary(console.gameObject);
        }

        Debug.Log($"Spawner: Odbudowano słownik. Zarejestrowano {puzzleObjects.Count} obiektów.");
    }

    private void RegisterObjectInDictionary(GameObject obj)
    {
        // Musimy odzyskać pozycję Vector2Int na podstawie pozycji w świecie
        Vector2Int gridPos = GetGridPositionFromWorld(obj.transform.position);

        if (!puzzleObjects.ContainsKey(gridPos))
        {
            puzzleObjects.Add(gridPos, obj);
        }
        else
        {
            // To może się zdarzyć, jeśli np. płytka i skrzynia są w tym samym miejscu 
            // Debug.LogWarning($"Duplikat na pozycji {gridPos}: {obj.name}");
        }
    }

    private void InitializePropLookup()
    {
        _propLookup = new Dictionary<PropWFC.PropType, WfcPropMapping>();
        foreach (var mapping in wfcPropMappings)
        {
            if (!_propLookup.ContainsKey(mapping.type))
                _propLookup.Add(mapping.type, mapping);
        }
    }

    public void SpawnWfcProp(Vector2Int position, PropWFC.PropType type)
    {
        if (_propLookup == null) 
        {
            InitializePropLookup();
        }

        if (type == PropWFC.PropType.Empty) return;

        if (_propLookup.TryGetValue(type, out WfcPropMapping mapping))
        {
            if (mapping.prefab != null)
            {

                float xPos = position.x + 0.5f;
                float yPos = position.y; 

                Vector3 worldPos = new Vector3(xPos, yPos, 0);
                
                float offX = Random.Range(-mapping.offsetRange, mapping.offsetRange);
                float offY = Random.Range(-mapping.offsetRange, mapping.offsetRange);
                
                worldPos += new Vector3(offX, offY, 0);
                worldPos.z = entityZPosition;

                GameObject instance = Instantiate(mapping.prefab, worldPos, Quaternion.identity, transform);
            }
            else
            {
                Debug.LogWarning($"[Spawner BŁĄD] Typ {type} jest w słowniku, ale PREFAB jest NULL!");
            }
        }
        else
        {
            Debug.LogError($"[Spawner BŁĄD] Nie znaleziono mapowania dla typu: {type}.");
        }
    }
    // Metoda pomocnicza - odwrotność GetCellCenterWorld
    private Vector2Int GetGridPositionFromWorld(Vector3 worldPos)
    {
        // Zakładamy, że Grid jest standardowy (1x1). 
        // Musimy odjąć offset (0.5), który dodawaliśmy przy spawnowaniu.
        
        int x = Mathf.FloorToInt(worldPos.x); // lub Mathf.RoundToInt(worldPos.x - 0.5f)
        int y = Mathf.FloorToInt(worldPos.y);
        
        // Najbezpieczniej użyć Tilemapy, jeśli mamy do niej dostęp
        if (dungeonGenerator != null && dungeonGenerator.TilemapVisualizer != null)
        {
            var tilemap = dungeonGenerator.TilemapVisualizer.FloorTilemap;
            if (tilemap != null)
            {
                Vector3Int cell = tilemap.WorldToCell(worldPos);
                return new Vector2Int(cell.x, cell.y);
            }
        }

        // Fallback: Prosta matematyka (zależy jak dokładnie centrujesz obiekty)
        // Jeśli obiekt jest na 10.5, 10.5 -> To Grid 10, 10
        return new Vector2Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.y));
    }

    // Metoda uniwersalna do spawnowania obiektu w idealnym centrum logicznej komórki
    private void SpawnObjectAtCellCenter(GameObject prefab, Vector2Int cellPosition, UnityEngine.Tilemaps.Tilemap floorTilemap)
    {
        // 1. Pobranie pozycji World Space dolnego lewego rogu komórki
        Vector3 cellWorldPosition = floorTilemap.CellToWorld(new Vector3Int(cellPosition.x, cellPosition.y, 0));
        
        // 2. Dodanie połowy rozmiaru komórki, aby znaleźć środek
        // Używamy cellSize, który automatycznie bierze pod uwagę wszelkie skalowanie siatki (np. Cell Size 0.5)
        Vector3 cellSize = floorTilemap.cellSize; 
        Vector3 centerOffset = new Vector3(cellSize.x / 2f, cellSize.y / 2f, 0);

        Vector3 spawnPosition = cellWorldPosition + centerOffset;

        // Ustawienie Z (zakładam, że masz już zdefiniowaną zmienną entityZPosition)
        spawnPosition.z = entityZPosition; 
        
        // Spawnowanie
        GameObject instance = Instantiate(prefab, spawnPosition, Quaternion.identity, transform);
        
        
        // Zapisz referencję w słowniku puzzleObjects (jeśli jest używany)
        if (prefab == gatePrefab || prefab == treasureChestPrefab || prefab == pressurePlatePrefab)
        {
            puzzleObjects[cellPosition] = instance; 
        }
    }

    public void SpawnPlayer(Vector2Int position)
    {
        if (currentPlayer != null)
        {
            Destroy(currentPlayer);
        }

        Vector3 spawnPosition = new Vector3(position.x, position.y, entityZPosition);
        currentPlayer = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        Debug.Log($"Player spawned at: {spawnPosition}");
    }

    public void SpawnBoss(Vector2Int position)
    {
        if (currentBoss != null)
        {
            Destroy(currentBoss);
        }

        Vector3 spawnPosition = new Vector3(position.x, position.y, entityZPosition);
        currentBoss = Instantiate(bossPrefab, spawnPosition, Quaternion.identity);
        Debug.Log($"Boss spawned at: {spawnPosition}");
    }

    // === NOWE METODY DLA ZAGADEK ===

    public void SpawnPressurePlate(Vector2Int position, RoomFirstDungeonGenerator generator) 
    {
        // 1. Zabezpieczenie przed duplikatami (jeśli już coś tu stoi)
        if (puzzleObjects.ContainsKey(position))
        {
            Debug.LogWarning($"SPAWNER: Próba zespawnowania płytki na ZAJĘTEJ pozycji {position}. Ignoruję.");
            return;
        }
        
        if (pressurePlatePrefab == null)
        {
            Debug.LogWarning("Brak prefabu płytki ciśnieniowej. Nie można spawnować.");
            return;
        }

        // 2. Obliczenie pozycji
        Vector3 spawnPosition = GetCellCenterWorld(position);
        spawnPosition.z = entityZPosition;

        // 3. Instancjonowanie (Tylko raz!)
        GameObject plateInstance = Instantiate(pressurePlatePrefab, spawnPosition, Quaternion.identity, transform);
        
        // 4. Pobranie i inicjalizacja komponentu
        PressurePlateComponent plateComponent = plateInstance.GetComponent<PressurePlateComponent>();

        PuzzleManager managerToUse = PuzzleManager; 
        
        // Fallback w poszukiwaniu managera
        if (managerToUse == null)
        {
            managerToUse = FindFirstObjectByType<PuzzleManager>();
        }

        if (plateComponent != null && managerToUse != null)
        {
            plateComponent.Initialize(managerToUse); 
        }
        else
        {
            Debug.LogError($"Błąd Spawnera: Nie można zainicjalizować płytki na {position}. Manager lub Komponent brakujący.");
        }
        
        // 5. Rejestracja obiektu w lokalnym słowniku (do usuwania)
        puzzleObjects[position] = plateInstance;

        // === 6. NOWOŚĆ: Rejestracja pozycji jako ZAJĘTEJ dla Prop Systemu ===
        if (managerToUse != null)
        {
            // Dzięki temu generator propów (kamieni, dekoracji) będzie wiedział, 
            // żeby ominąć to pole.
            managerToUse.ReservedPuzzlePositions.Add(position);
        }
    }

    public void SpawnTreasureChest(Vector2Int position, RoomFirstDungeonGenerator generator)
    {
        if (treasureChestPrefab == null) return;
    
        // 1. Zabezpieczenie: Jeśli coś tu już stoi (np. gracz wbiegł na środek), nie spawnuj "w nim"
        // LUB po prostu usuń stary obiekt jeśli to np. dekoracja.
        if (puzzleObjects.ContainsKey(position))
        {
            Debug.LogWarning($"Na pozycji {position} już coś stoi. Skrzynia może się nałożyć.");
            // Opcjonalnie: return; 
        }

        // 2. Obliczenie pozycji (środek kafelka)
        Vector3 spawnPosition = GetCellCenterWorld(position);
        spawnPosition.z = entityZPosition;

        // 3. Instancjonowanie
        GameObject chestInstance = Instantiate(treasureChestPrefab, spawnPosition, Quaternion.identity, transform);
        
        // 4. Rejestracja w systemach
        puzzleObjects[position] = chestInstance;
        
        if (PuzzleManager != null)
        {
            PuzzleManager.ReservedPuzzlePositions.Add(position);
        }
        
        // === OPCJONALNIE: Efekt pojawienia się (VFX) ===
        // Jeśli masz prefab efektu (np. dym, błysk), możesz go tu dodać:
        // Instantiate(spawnPoofVFX, spawnPosition, Quaternion.identity);
        
        Debug.Log($"Nagroda zespawnowana na pozycji: {position}");
    }

    public void ActivateTreasureChest(Vector2Int position)
    {
        if (puzzleObjects.ContainsKey(position))
        {
            GameObject chest = puzzleObjects[position];
            // Załóżmy, że skrypt TreasureChestComponent otwiera/uaktywnia nagrodę
            // chest.GetComponent<TreasureChestComponent>().Unlock(); 
            Debug.Log($"Skrzynia na pozycji {position} została odblokowana.");
        }
    }
    
    public void SpawnGate(Vector2Int position, RoomFirstDungeonGenerator generator, bool isSolved) 
    {
        if (gatePrefab == null) return;
        if (puzzleObjects.ContainsKey(position)) return;

        UnityEngine.Tilemaps.Tilemap floorTilemap = generator.TilemapVisualizer.FloorTilemap;

        // 1. Detekcja Orientacji (tak jak wcześniej)
        bool hasFloorLeft = floorTilemap.HasTile(new Vector3Int(position.x - 1, position.y, 0));
        bool hasFloorRight = floorTilemap.HasTile(new Vector3Int(position.x + 1, position.y, 0));
        
        // Jeśli sąsiedzi to ściany (brak podłogi po bokach), to korytarz jest PIONOWY
        bool isHorizontal = (hasFloorLeft || hasFloorRight);

        // 2. Obliczenie pozycji
        Vector3 spawnPosition = GetCellCenterWorld(position);
        spawnPosition.z = entityZPosition;

        // 3. Instancjonowanie (Bez rotacji! Sprite załatwi sprawę)
        GameObject gateInstance = Instantiate(gatePrefab, spawnPosition, Quaternion.identity, transform);
        
        // 4. Konfiguracja Komponentu
        GateComponent gateScript = gateInstance.GetComponent<GateComponent>();
        if (gateScript != null)
        {
            // Przekazujemy orientację i czy mają być od razu otwarte
            gateScript.Initialize(isHorizontal, isSolved);
        }
        else
        {
            Debug.LogError("Prefab Bramy nie ma skryptu GateComponent!");
        }

        // 5. Rejestracja
        puzzleObjects[position] = gateInstance;
        
        if (PuzzleManager != null)
        {
            PuzzleManager.ReservedPuzzlePositions.Add(position);
        }
    }

    public void OpenGate(Vector2Int position)
    {
        Debug.Log($"[SPAWNER DIAGNOSTYKA] Próba otwarcia bramy na pozycji: {position}");

        // 1. Sprawdzenie czy pozycja istnieje w słowniku
        if (puzzleObjects.ContainsKey(position))
        {
            GameObject gateObj = puzzleObjects[position];
            
            if (gateObj == null)
            {
                Debug.LogError($"[SPAWNER ERROR] Klucz {position} istnieje, ale GameObject jest NULL (został zniszczony?)");
                return;
            }

            Debug.Log($"[SPAWNER] Znaleziono obiekt: {gateObj.name} na pozycji {position}. Szukam komponentu...");

            GateComponent gateScript = gateObj.GetComponent<GateComponent>();
            
            if (gateScript != null)
            {
                Debug.Log("[SPAWNER] Komponent GateComponent znaleziony! Wywołuję Open()...");
                gateScript.Open(); 
            }
            else
            {
                Debug.LogError($"[SPAWNER ERROR] Obiekt '{gateObj.name}' nie ma skryptu GateComponent! Sprawdź prefab.");
            }
        }
        else
        {
            // === NAJWAŻNIEJSZA CZĘŚĆ DIAGNOSTYKI ===
            Debug.LogError($"[SPAWNER ERROR] Nie znaleziono obiektu na pozycji {position} w słowniku puzzleObjects!");
            
            // Wypiszmy co faktycznie JEST w słowniku, żeby znaleźć ewentualny błąd o 1 kratkę
            Debug.Log("--- ZAWARTOŚĆ SŁOWNIKA PUZZLE OBJECTS ---");
            foreach (var kvp in puzzleObjects)
            {
                // Filtrujemy, żeby pokazać tylko bramy (jeśli mają w nazwie Gate lub Clone)
                if (kvp.Value != null && kvp.Value.name.Contains("Gate"))
                {
                    Debug.Log($"Zarejestrowana Brama: {kvp.Key} | Obiekt: {kvp.Value.name}");
                }
            }
            Debug.Log("-------------------------------------------");
        }
    }
    public void RemoveGate(Vector2Int position)
    {
        if (puzzleObjects.ContainsKey(position))
        {
            Destroy(puzzleObjects[position]);
            puzzleObjects.Remove(position);
            Debug.Log($"Brama na pozycji {position} została usunięta.");
        }
    }
    
    // === ZMODYFIKOWANA METODA CLEAR ===

    public void ClearSpawns()
    {
        if (currentPlayer != null) Destroy(currentPlayer);
        if (currentBoss != null) Destroy(currentBoss);

        // Usuń wszystkie obiekty związane z zagadkami
        foreach (var obj in puzzleObjects.Values)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        puzzleObjects.Clear();
        
        Debug.Log("Wyczyszczono wszystkie obiekty (gracz, boss, zagadki).");
    }

    /// Spawnuje prop w określonej pozycji z losowym przesunięciem.
    public void SpawnProp(RoomPropData.PropEntry propEntry, Vector2Int position)
    {
        if (propEntry.propPrefab == null) return;
        
        // Konwersja pozycji kafelka na pozycję World Space (środek kafelka)
        Vector3 worldPosition = GetCellCenterWorld(position);

        // Dodanie losowego przesunięcia (jitter)
        float offset = propEntry.randomOffsetRange;
        worldPosition.x += Random.Range(-offset, offset);
        worldPosition.y += Random.Range(-offset, offset);
        
        // Instancjonowanie
        GameObject propInstance = Instantiate(propEntry.propPrefab, worldPosition, Quaternion.identity);

        // Opcjonalnie: Zagnieżdżenie pod obiektem nadrzędnym (dla porządku w hierarchii)
        propInstance.transform.SetParent(transform); 
    }

    private Vector3 GetCellCenterWorld(Vector2Int position)
    {
        var tilemapVisualizer = DungeonGenerator?.TilemapVisualizer;

        var floorTilemap = tilemapVisualizer?.FloorTilemap;

        if (floorTilemap != null)
        {
            return floorTilemap.GetCellCenterWorld(new Vector3Int(position.x, position.y, 0));
        }
        
        return new Vector3(position.x + 0.5f, position.y + 0.5f, 0); 
    }
    public void SpawnBinaryConsole(Vector2Int position, PuzzleData puzzleData)
    {
        if (puzzleObjects.ContainsKey(position))
        {
            Debug.LogWarning($"SPAWNER: Próba zespawnowania konsoli na ZAJĘTEJ pozycji {position}. Ignoruję.");
            return;
        }

        if (binaryConsolePrefab == null)
        {
            Debug.LogError("BinaryConsolePrefab nie jest przypisany!");
            return;
        }

        if (DungeonGenerator == null)
        {
            Debug.LogError("FATAL ERROR: DungeonGenerator został utracony w Spawnerze! Próba odzyskania referencji...");
            
            // Spróbuj odzyskać referencję z hierarchii (jeśli Spawner jest na tym samym obiekcie co Generator)
            // LUB jeśli jest to Game Manager:
            DungeonGenerator = FindFirstObjectByType<RoomFirstDungeonGenerator>();
            
            if (DungeonGenerator == null)
            {
                Debug.LogError("Nie można odzyskać referencji DungeonGenerator. Przerwanie spawnowania.");
                return; 
            }
        }
        // 1. Zabezpieczony dostęp do Tilemapy (dla logiki CellToWorld)
        UnityEngine.Tilemaps.Tilemap floorTilemap = DungeonGenerator?.TilemapVisualizer?.FloorTilemap;
        
        Vector3 spawnPosition;

        if (floorTilemap != null)
        {
            // === NOWE, BEZPIECZNE OBLICZENIE WORLD POSITION ===
            // Używamy CellToWorld i CellSize, co jest bardziej niezawodne niż GetCellCenterWorld
            
            Vector3 cellWorldPosition = floorTilemap.CellToWorld(new Vector3Int(position.x, position.y, 0));
            Vector3 cellSize = floorTilemap.cellSize; 
            Vector3 centerOffset = new Vector3(cellSize.x / 2f, cellSize.y / 2f, 0);

            spawnPosition = cellWorldPosition + centerOffset;
            spawnPosition.z = entityZPosition;
        }
        else
        {
            // Fallback: spawnuje na nieprecyzyjnej pozycji World Space z offsetem 0.5
            spawnPosition = new Vector3(position.x + 0.5f, position.y + 0.5f, entityZPosition);
            Debug.LogError("Brak FloorTilemap! Konsola spawnuje się na nieprecyzyjnej pozycji World Space.");
        }

        GameObject consoleInstance = Instantiate(binaryConsolePrefab, spawnPosition, Quaternion.identity);
        
        BinaryConsoleComponent consoleComponent = consoleInstance.GetComponent<BinaryConsoleComponent>();
        
        if (consoleComponent == null)
        {
            Debug.LogError($"FATAL ERROR: Prefab '{binaryConsolePrefab.name}' nie posiada skryptu BinaryConsoleComponent.cs!");
            return; 
        }

        // ... (reszta logiki zabezpieczającej isKeyPuzzle) ...
        bool isKeyPuzzle = false;
        
        if (DungeonGenerator != null && DungeonGenerator.allRooms != null && DungeonGenerator.MainPathRooms != null)
        {
            isKeyPuzzle = DungeonGenerator.MainPathRooms.Contains(
                DungeonGenerator.FindRoomContainingPoint(DungeonGenerator.allRooms, position)
            );
        } 
        
        consoleComponent.Initialize(puzzleData, this, isKeyPuzzle);
        
        // Zapisz pozycję w słowniku zarezerwowanych obiektów
        puzzleObjects[position] = consoleInstance;
        
        // Aktualizuj zbiór zarezerwowanych pozycji
        PuzzleManager.ReservedPuzzlePositions.Add(position); 
    }
    public void RemovePuzzleObject(Vector2Int position)
    {
        if (puzzleObjects.ContainsKey(position))
        {
            GameObject objToRemove = puzzleObjects[position];
            puzzleObjects.Remove(position);
            
            // Użyj Destroy, aby usunąć obiekt z gry
            if (objToRemove != null)
            {
                Destroy(objToRemove);
            }
            
            Debug.Log($"Obiekt zagadki na pozycji {position} został usunięty.");
        }
    }
}