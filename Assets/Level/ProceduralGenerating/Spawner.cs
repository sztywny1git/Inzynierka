using UnityEngine;
using System.Collections.Generic;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    private GameObject playerPrefab;
    [SerializeField]
    private GameObject bossPrefab;

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

    [SerializeField] 
    private PuzzleManager puzzleManager;

    public PuzzleManager PuzzleManager => puzzleManager;
    [SerializeField] private RoomFirstDungeonGenerator dungeonGenerator; 
    public RoomFirstDungeonGenerator DungeonGenerator { get => dungeonGenerator; set => dungeonGenerator = value; }

    [System.Serializable]
    public struct WfcPropMapping
    {
        public PropWFC.PropType type;
        public GameObject prefab;
        public float offsetRange; 
    }

    [Header("WFC Props Configuration")]
    [SerializeField] private List<WfcPropMapping> wfcPropMappings;
    private Dictionary<PropWFC.PropType, WfcPropMapping> _propLookup;

    public void Initialize(PuzzleManager manager)
    {
        puzzleManager = manager;
    }
    private GameObject currentPlayer;
    private GameObject currentBoss;
    
    private Dictionary<Vector2Int, GameObject> puzzleObjects = new Dictionary<Vector2Int, GameObject>();

    private void Awake()    
    {
        Debug.unityLogger.logEnabled = false;
        if (puzzleObjects.Count == 0)
        {
            RebuildObjectDictionary();
        }
    }

    private void RebuildObjectDictionary()
    {
        //Debug.Log("Spawner: Odbudowywanie słownika obiektów (Rebuild)...");

        // 1. Znajdź wszystkie BRAMY będące dziećmi tego Spawnera
        GateComponent[] gates = GetComponentsInChildren<GateComponent>(true); 
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
        
        // Opcjonalnie: Znajdź konsole
        BinaryConsoleComponent[] consoles = GetComponentsInChildren<BinaryConsoleComponent>(true);
        foreach (var console in consoles)
        {
            RegisterObjectInDictionary(console.gameObject);
        }

        // Debug.Log($"Spawner: Odbudowano słownik. Zarejestrowano {puzzleObjects.Count} obiektów.");
    }

    private void RegisterObjectInDictionary(GameObject obj)
    {
        Vector2Int gridPos = GetGridPositionFromWorld(obj.transform.position);

        if (!puzzleObjects.ContainsKey(gridPos))
        {
            puzzleObjects.Add(gridPos, obj);
        }
        else
        {
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
    private Vector2Int GetGridPositionFromWorld(Vector3 worldPos)
    {
        
        int x = Mathf.FloorToInt(worldPos.x); 
        int y = Mathf.FloorToInt(worldPos.y);
        
        if (dungeonGenerator != null && dungeonGenerator.TilemapVisualizer != null)
        {
            var tilemap = dungeonGenerator.TilemapVisualizer.FloorTilemap;
            if (tilemap != null)
            {
                Vector3Int cell = tilemap.WorldToCell(worldPos);
                return new Vector2Int(cell.x, cell.y);
            }
        }

        return new Vector2Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.y));
    }

    private void SpawnObjectAtCellCenter(GameObject prefab, Vector2Int cellPosition, UnityEngine.Tilemaps.Tilemap floorTilemap)
    {
        Vector3 cellWorldPosition = floorTilemap.CellToWorld(new Vector3Int(cellPosition.x, cellPosition.y, 0));
        
        Vector3 cellSize = floorTilemap.cellSize; 
        Vector3 centerOffset = new Vector3(cellSize.x / 2f, cellSize.y / 2f, 0);

        Vector3 spawnPosition = cellWorldPosition + centerOffset;

        spawnPosition.z = entityZPosition; 

        GameObject instance = Instantiate(prefab, spawnPosition, Quaternion.identity, transform);
        
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
        //Debug.Log($"Player spawned at: {spawnPosition}");
    }

    public void SpawnBoss(Vector2Int position)
    {
        if (currentBoss != null)
        {
            Destroy(currentBoss);
        }

        Vector3 spawnPosition = new Vector3(position.x, position.y, entityZPosition);
        currentBoss = Instantiate(bossPrefab, spawnPosition, Quaternion.identity);
        //Debug.Log($"Boss spawned at: {spawnPosition}");
    }

    public void SpawnPressurePlate(Vector2Int position, RoomFirstDungeonGenerator generator) 
    {
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

        Vector3 spawnPosition = GetCellCenterWorld(position);
        spawnPosition.z = entityZPosition;

        GameObject plateInstance = Instantiate(pressurePlatePrefab, spawnPosition, Quaternion.identity, transform);
        
        PressurePlateComponent plateComponent = plateInstance.GetComponent<PressurePlateComponent>();

        PuzzleManager managerToUse = PuzzleManager; 
        
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
        
        puzzleObjects[position] = plateInstance;

        if (managerToUse != null)
        {
            managerToUse.ReservedPuzzlePositions.Add(position);
        }
    }

    public void SpawnTreasureChest(Vector2Int position, RoomFirstDungeonGenerator generator)
    {
        if (treasureChestPrefab == null) return;
    
        if (puzzleObjects.ContainsKey(position))
        {
            //Debug.LogWarning($"Na pozycji {position} już coś stoi. Skrzynia może się nałożyć.");
        }

        Vector3 spawnPosition = GetCellCenterWorld(position);
        spawnPosition.z = entityZPosition;

        GameObject chestInstance = Instantiate(treasureChestPrefab, spawnPosition, Quaternion.identity, transform);
        
        puzzleObjects[position] = chestInstance;
        
        if (PuzzleManager != null)
        {
            PuzzleManager.ReservedPuzzlePositions.Add(position);
        }
        
        //Debug.Log($"Nagroda zespawnowana na pozycji: {position}");
    }

    public void ActivateTreasureChest(Vector2Int position)
    {
        if (puzzleObjects.ContainsKey(position))
        {
            GameObject chest = puzzleObjects[position];
            //Debug.Log($"Skrzynia na pozycji {position} została odblokowana.");
        }
    }
    
    public void SpawnGate(Vector2Int position, RoomFirstDungeonGenerator generator, bool isSolved) 
    {
        if (gatePrefab == null) return;
        if (puzzleObjects.ContainsKey(position)) return;

        UnityEngine.Tilemaps.Tilemap floorTilemap = generator.TilemapVisualizer.FloorTilemap;

        bool hasFloorLeft = floorTilemap.HasTile(new Vector3Int(position.x - 1, position.y, 0));
        bool hasFloorRight = floorTilemap.HasTile(new Vector3Int(position.x + 1, position.y, 0));
        
        bool isHorizontal = (hasFloorLeft || hasFloorRight);

        Vector3 spawnPosition = GetCellCenterWorld(position);
        spawnPosition.z = entityZPosition;

        GameObject gateInstance = Instantiate(gatePrefab, spawnPosition, Quaternion.identity, transform);
        
        GateComponent gateScript = gateInstance.GetComponent<GateComponent>();
        if (gateScript != null)
        {
            gateScript.Initialize(isHorizontal, isSolved);
        }
        else
        {
            Debug.LogError("Prefab Bramy nie ma skryptu GateComponent!");
        }

        puzzleObjects[position] = gateInstance;
        
        if (PuzzleManager != null)
        {
            PuzzleManager.ReservedPuzzlePositions.Add(position);
        }
    }

    public void OpenGate(Vector2Int position)
    {
        //Debug.Log($"[SPAWNER DIAGNOSTYKA] Próba otwarcia bramy na pozycji: {position}");

        if (puzzleObjects.ContainsKey(position))
        {
            GameObject gateObj = puzzleObjects[position];
            
            if (gateObj == null)
            {
                Debug.LogError($"[SPAWNER ERROR] Klucz {position} istnieje, ale GameObject jest NULL (został zniszczony?)");
                return;
            }

            //Debug.Log($"[SPAWNER] Znaleziono obiekt: {gateObj.name} na pozycji {position}. Szukam komponentu...");

            GateComponent gateScript = gateObj.GetComponent<GateComponent>();
            
            if (gateScript != null)
            {
                //Debug.Log("[SPAWNER] Komponent GateComponent znaleziony! Wywołuję Open()...");
                gateScript.Open(); 
            }
            else
            {
                //Debug.LogError($"[SPAWNER ERROR] Obiekt '{gateObj.name}' nie ma skryptu GateComponent! Sprawdź prefab.");
            }
        }
        else
        {
            //Debug.LogError($"[SPAWNER ERROR] Nie znaleziono obiektu na pozycji {position} w słowniku puzzleObjects!");
            
            //Debug.Log("--- ZAWARTOŚĆ SŁOWNIKA PUZZLE OBJECTS ---");
            foreach (var kvp in puzzleObjects)
            {
                if (kvp.Value != null && kvp.Value.name.Contains("Gate"))
                {
                    //Debug.Log($"Zarejestrowana Brama: {kvp.Key} | Obiekt: {kvp.Value.name}");
                }
            }
            //Debug.Log("-------------------------------------------");
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

    public void ClearSpawns()
    {
        if (currentPlayer != null) Destroy(currentPlayer);
        if (currentBoss != null) Destroy(currentBoss);

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

    public void SpawnProp(RoomPropData.PropEntry propEntry, Vector2Int position)
    {
        if (propEntry.propPrefab == null) return;
        
        Vector3 worldPosition = GetCellCenterWorld(position);

        float offset = propEntry.randomOffsetRange;
        worldPosition.x += Random.Range(-offset, offset);
        worldPosition.y += Random.Range(-offset, offset);
        
        GameObject propInstance = Instantiate(propEntry.propPrefab, worldPosition, Quaternion.identity);

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
            
            DungeonGenerator = FindFirstObjectByType<RoomFirstDungeonGenerator>();
            
            if (DungeonGenerator == null)
            {
                Debug.LogError("Nie można odzyskać referencji DungeonGenerator. Przerwanie spawnowania.");
                return; 
            }
        }

        UnityEngine.Tilemaps.Tilemap floorTilemap = DungeonGenerator?.TilemapVisualizer?.FloorTilemap;
        
        Vector3 spawnPosition;

        if (floorTilemap != null)
        {
            
            Vector3 cellWorldPosition = floorTilemap.CellToWorld(new Vector3Int(position.x, position.y, 0));
            Vector3 cellSize = floorTilemap.cellSize; 
            Vector3 centerOffset = new Vector3(cellSize.x / 2f, cellSize.y / 2f, 0);

            spawnPosition = cellWorldPosition + centerOffset;
            spawnPosition.z = entityZPosition;
        }
        else
        {
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

        bool isKeyPuzzle = false;
        
        if (DungeonGenerator != null && DungeonGenerator.allRooms != null && DungeonGenerator.MainPathRooms != null)
        {
            isKeyPuzzle = DungeonGenerator.MainPathRooms.Contains(
                DungeonGenerator.FindRoomContainingPoint(DungeonGenerator.allRooms, position)
            );
        } 
        
        consoleComponent.Initialize(puzzleData, this, isKeyPuzzle);
        
        puzzleObjects[position] = consoleInstance;
        
        PuzzleManager.ReservedPuzzlePositions.Add(position); 
    }
    public void RemovePuzzleObject(Vector2Int position)
    {
        if (puzzleObjects.ContainsKey(position))
        {
            GameObject objToRemove = puzzleObjects[position];
            puzzleObjects.Remove(position);
            
            if (objToRemove != null)
            {
                Destroy(objToRemove);
            }
            
            Debug.Log($"Obiekt zagadki na pozycji {position} został usunięty.");
        }
    }
}