using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using VContainer;
using VContainer.Unity;

public class EnemySpawner : MonoBehaviour
{
    public enum SpawnMode
    {
        FromDungeonGenerator,
        FromTilemap,
        FromManualPoints
    }
    
    [Serializable]
    public struct EnemyWeightEntry
    {
        public GameObject prefab;
        [Min(1)] public int weight;
    }

    [Header("Spawn Mode")]
    [SerializeField] private SpawnMode spawnMode = SpawnMode.FromDungeonGenerator;
    [SerializeField] private Tilemap manualFloorTilemap;
    [SerializeField] private Transform[] manualSpawnPoints;
    
    [Header("Density Settings")]
    [Tooltip("How many enemies per tile. 0.01 = 1 enemy per 100 tiles.")]
    [SerializeField] [Range(0f, 0.1f)] private float globalDensity = 0.02f;

    [Header("Enemy Types & Weights")]
    [SerializeField] private List<EnemyWeightEntry> enemyTable = new List<EnemyWeightEntry>();

    [Header("Boss Settings")]
    [SerializeField] private GameObject bossPrefab;

    [Header("When")]
    [SerializeField] private bool spawnOnStart = false;

    [Header("Placement")]
    [SerializeField] private float minDistanceBetweenEnemies = 1.0f;
    [SerializeField] private float spawnCheckRadius = 0.35f;
    [SerializeField] private LayerMask spawnBlockMask = ~0;
    [SerializeField] private float zPosition = 0f;

    [Header("Attempts")]
    [SerializeField] private int attemptsPerSpawn = 20;

    [Header("Housekeeping")]
    [SerializeField] private bool clearPreviousSpawns = true;

    [Header("Enemy Scaling")]
    [SerializeField] private bool enableScaling = true;
    
    [SerializeField] private int currentLevel = 1;
    
    [SerializeField] private ExpManager expManager;

    private RoomFirstDungeonGenerator _generator;
    private Tilemap _floorTilemap;
    private HashSet<Vector2Int> _floorPositions;
    private readonly List<GameObject> _spawned = new List<GameObject>(128);
    private GameObject _activeBoss;
    private int _totalWeight;

    private IObjectResolver _container;

    [Inject]
    public void Construct(IObjectResolver container)
    {
        _container = container;
    }

    private void Awake()
    {
        _generator = FindFirstObjectByType<RoomFirstDungeonGenerator>();
        _floorTilemap = _generator != null && _generator.TilemapVisualizer != null
            ? _generator.TilemapVisualizer.FloorTilemap
            : manualFloorTilemap;
            
        CalculateTotalWeight();
    }

    private void Start()
    {
        if (spawnOnStart)
        {
            SpawnEnemies();
        }
    }
    
    private void OnValidate()
    {
        CalculateTotalWeight();
    }

    private void CalculateTotalWeight()
    {
        _totalWeight = 0;
        foreach (var entry in enemyTable)
        {
            _totalWeight += entry.weight;
        }
    }

    public void SpawnEnemies()
    {
        if (clearPreviousSpawns)
        {
            ClearSpawned();
        }

        bool initialized = false;
        
        switch (spawnMode)
        {
            case SpawnMode.FromDungeonGenerator:
                initialized = InitializeFromDungeonGenerator();
                break;
                
            case SpawnMode.FromTilemap:
                initialized = InitializeFromTilemap();
                break;
                
            case SpawnMode.FromManualPoints:
                initialized = InitializeFromManualPoints();
                break;
        }
        
        if (!initialized)
        {
            Debug.LogWarning($"[EnemySpawner] Failed to initialize spawn positions (mode: {spawnMode}). Nothing spawned.");
            return;
        }

        if (enemyTable.Count == 0)
        {
            Debug.LogWarning("[EnemySpawner] No enemies in Enemy Table!");
            return;
        }

        int levelForScaling = GetCurrentLevel();

        if (spawnMode == SpawnMode.FromDungeonGenerator && _generator != null)
        {
            SpawnInDungeonRooms(levelForScaling);
        }
        else
        {
            SpawnGlobal(levelForScaling);
        }
    }

    public void SpawnBoss(Vector2Int gridPosition)
    {
        if (bossPrefab == null)
        {
            Debug.LogWarning("[EnemySpawner] Boss Prefab is not assigned!");
            return;
        }

        if (_activeBoss != null)
        {
            Destroy(_activeBoss);
        }

        Vector3 worldPos = GetCellCenterWorld(gridPosition);
        worldPos.z = zPosition;

        if (_container != null)
        {
            _activeBoss = _container.Instantiate(bossPrefab, worldPos, Quaternion.identity, transform);
        }
        else
        {
            _activeBoss = Instantiate(bossPrefab, worldPos, Quaternion.identity, transform);
        }

        if (enableScaling)
        {
            int level = GetCurrentLevel();
            ApplyScalingToEnemy(_activeBoss, level);
        }
    }

    public void ClearSpawned()
    {
        for (int i = _spawned.Count - 1; i >= 0; i--)
        {
            if (_spawned[i] != null)
            {
                Destroy(_spawned[i]);
            }
        }
        _spawned.Clear();
    }

    public void ClearAll()
    {
        ClearSpawned();
        if (_activeBoss != null)
        {
            Destroy(_activeBoss);
            _activeBoss = null;
        }
    }

    private void SpawnInDungeonRooms(int level)
    {
        if (_generator.allRooms == null) return;

        foreach (var room in _generator.allRooms)
        {
            RoomType type = _generator.GetRoomType(room);

            if (type == RoomType.Start || type == RoomType.Boss || type == RoomType.Puzzle)
                continue;

            int roomArea = room.size.x * room.size.y;
            int targetCount = Mathf.RoundToInt(roomArea * globalDensity);

            if (targetCount <= 0) continue;

            SpawnEnemiesInArea(targetCount, room, level);
        }
    }

    private void SpawnGlobal(int level)
    {
        int totalArea = _floorPositions.Count;
        int targetCount = Mathf.RoundToInt(totalArea * globalDensity);

        if (targetCount <= 0) return;
            
        BoundsInt globalBounds = new BoundsInt();
        if (_floorTilemap != null)
        {
            globalBounds = _floorTilemap.cellBounds;
        }
        else
        {
            globalBounds.min = new Vector3Int(-100, -100, 0);
            globalBounds.max = new Vector3Int(100, 100, 0);
        }

        SpawnEnemiesInArea(targetCount, globalBounds, level);
    }

    private void SpawnEnemiesInArea(int count, BoundsInt searchBounds, int level)
    {
        float minEnemySqr = minDistanceBetweenEnemies * minDistanceBetweenEnemies;

        for (int i = 0; i < count; i++)
        {
            GameObject prefabToSpawn = GetRandomEnemyPrefab();
            if (prefabToSpawn == null) continue;

            for (int attempt = 0; attempt < attemptsPerSpawn; attempt++)
            {
                if (TryPickPositionInBounds(searchBounds, out Vector3 candidate))
                {
                    if (IsPositionValid(candidate, minEnemySqr))
                    {
                        GameObject instance;
                        if (_container != null)
                        {
                            instance = _container.Instantiate(prefabToSpawn, candidate, Quaternion.identity, transform);
                        }
                        else
                        {
                            instance = Instantiate(prefabToSpawn, candidate, Quaternion.identity, transform);
                        }

                        _spawned.Add(instance);

                        if (enableScaling)
                        {
                            ApplyScalingToEnemy(instance, level);
                        }
                        break;
                    }
                }
            }
        }
    }

    private GameObject GetRandomEnemyPrefab()
    {
        if (_totalWeight <= 0) return null;

        int randomValue = UnityEngine.Random.Range(0, _totalWeight);
        int currentWeight = 0;

        foreach (var entry in enemyTable)
        {
            currentWeight += entry.weight;
            if (randomValue < currentWeight)
            {
                return entry.prefab;
            }
        }

        return enemyTable[0].prefab;
    }

    private bool TryPickPositionInBounds(BoundsInt bounds, out Vector3 position)
    {
        position = Vector3.zero;
        
        int x = UnityEngine.Random.Range(bounds.xMin, bounds.xMax);
        int y = UnityEngine.Random.Range(bounds.yMin, bounds.yMax);
        Vector2Int cell = new Vector2Int(x, y);

        if (_floorPositions.Contains(cell))
        {
            position = GetCellCenterWorld(cell);
            position.z = zPosition;
            return true;
        }

        return false;
    }

    private bool IsPositionValid(Vector3 candidate, float minEnemySqr)
    {
        for (int i = 0; i < _spawned.Count; i++)
        {
            if (_spawned[i] == null) continue;
            if ((candidate - _spawned[i].transform.position).sqrMagnitude < minEnemySqr)
            {
                return false;
            }
        }

        if (spawnCheckRadius > 0f)
        {
            var hit = Physics2D.OverlapCircle(candidate, spawnCheckRadius, spawnBlockMask);
            if (hit != null) return false;
        }

        return true;
    }

    private int GetCurrentLevel()
    {
        if (expManager != null)
        {
            return Mathf.Max(1, expManager.level);
        }
        return Mathf.Max(1, currentLevel);
    }

    public void SetLevel(int level)
    {
        currentLevel = Mathf.Max(1, level);
    }

    private void ApplyScalingToEnemy(GameObject enemy, int level)
    {
        var scaler = enemy.GetComponent<EnemyScaler>();
        if (scaler != null)
        {
            scaler.ApplyLevelScaling(level);
        }
    }
    
    private bool InitializeFromDungeonGenerator()
    {
        if (_generator == null)
        {
            _generator = FindFirstObjectByType<RoomFirstDungeonGenerator>();
        }

        if (_generator == null || _generator.floorPositions == null || _generator.floorPositions.Count == 0)
        {
            return false;
        }
        else
        {
            _floorPositions = _generator.floorPositions;
            _floorTilemap = _generator.TilemapVisualizer != null ? _generator.TilemapVisualizer.FloorTilemap : _floorTilemap;
            return true;
        }
    }
    
    private bool InitializeFromTilemap()
    {
        if (manualFloorTilemap == null)
        {
            Debug.LogError("[EnemySpawner] Manual floor tilemap not assigned!");
            return false;
        }
        
        _floorTilemap = manualFloorTilemap;
        _floorPositions = new HashSet<Vector2Int>();
        
        BoundsInt bounds = manualFloorTilemap.cellBounds;
        foreach (var pos in bounds.allPositionsWithin)
        {
            if (manualFloorTilemap.HasTile(pos))
            {
                _floorPositions.Add(new Vector2Int(pos.x, pos.y));
            }
        }
        
        return _floorPositions.Count > 0;
    }
    
    private bool InitializeFromManualPoints()
    {
        if (manualSpawnPoints == null || manualSpawnPoints.Length == 0)
        {
            Debug.LogError("[EnemySpawner] No manual spawn points assigned!");
            return false;
        }
        
        _floorPositions = new HashSet<Vector2Int>();
        foreach (var point in manualSpawnPoints)
        {
            if (point != null)
            {
                _floorPositions.Add(new Vector2Int(
                    Mathf.RoundToInt(point.position.x),
                    Mathf.RoundToInt(point.position.y)
                ));
            }
        }
        
        return _floorPositions.Count > 0;
    }

    private Vector3 GetCellCenterWorld(Vector2Int cell)
    {
        if (_floorTilemap != null)
        {
            Vector3Int tileCell = new Vector3Int(cell.x, cell.y, 0);
            Vector3 cellWorld = _floorTilemap.CellToWorld(tileCell);
            Vector3 size = _floorTilemap.cellSize;
            return cellWorld + new Vector3(size.x / 2f, size.y / 2f, 0f);
        }

        return new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, spawnCheckRadius);
    }
#endif
}