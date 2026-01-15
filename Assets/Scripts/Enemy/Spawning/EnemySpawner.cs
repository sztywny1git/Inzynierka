// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Tilemaps;

// public class EnemySpawner : MonoBehaviour
// {
//     public enum SpawnMode
//     {
//         FromDungeonGenerator,
//         FromTilemap,
//         FromManualPoints
//     }
    
//     [Serializable]
//     public struct SpawnEntry
//     {
//         public GameObject prefab;
//         [Min(0)] public int count;
//     }

//     [Header("Spawn Mode")]
//     [SerializeField] private SpawnMode spawnMode = SpawnMode.FromDungeonGenerator;
//     [SerializeField] private Tilemap manualFloorTilemap;
//     [SerializeField] private Transform[] manualSpawnPoints;
    
//     [Header("What to spawn")]
//     [SerializeField] private List<SpawnEntry> enemiesToSpawn = new List<SpawnEntry>();

//     [Header("When")]
//     [SerializeField] private bool spawnOnStart = true;
//     [SerializeField] private float startDelaySeconds = 0.25f;

//     [Header("Placement")]
//     [SerializeField] private float minDistanceFromPlayer = 6f;
//     [SerializeField] private float minDistanceBetweenEnemies = 1.0f;
//     [SerializeField] private float spawnCheckRadius = 0.35f;
//     [SerializeField] private LayerMask spawnBlockMask = ~0;
//     [SerializeField] private float zPosition = 0f;

//     [Header("Attempts")]
//     [SerializeField] private int attemptsPerEnemy = 30;

//     [Header("Housekeeping")]
//     [SerializeField] private bool clearPreviousSpawns = true;

//     private RoomFirstDungeonGenerator _generator;
//     private Tilemap _floorTilemap;
//     private HashSet<Vector2Int> _floorPositions;
//     private readonly List<GameObject> _spawned = new List<GameObject>(128);

//     private void Awake()
//     {
//         _generator = FindFirstObjectByType<RoomFirstDungeonGenerator>();
//         _floorTilemap = _generator != null && _generator.TilemapVisualizer != null
//             ? _generator.TilemapVisualizer.FloorTilemap
//             : manualFloorTilemap;
//     }

//     private void Start()
//     {
//         if (spawnOnStart)
//         {
//             StartCoroutine(SpawnRoutine());
//         }
//     }

//     public void SpawnNow()
//     {
//         StartCoroutine(SpawnRoutine());
//     }

//     public void ClearSpawned()
//     {
//         for (int i = _spawned.Count - 1; i >= 0; i--)
//         {
//             if (_spawned[i] != null)
//             {
//                 Destroy(_spawned[i]);
//             }
//         }
//         _spawned.Clear();
//     }

//     private IEnumerator SpawnRoutine()
//     {
//         if (startDelaySeconds > 0f)
//         {
//             yield return new WaitForSeconds(startDelaySeconds);
//         }

//         // Initialize floor positions based on spawn mode
//         bool initialized = false;
        
//         switch (spawnMode)
//         {
//             case SpawnMode.FromDungeonGenerator:
//                 yield return StartCoroutine(InitializeFromDungeonGenerator());
//                 initialized = _floorPositions != null && _floorPositions.Count > 0;
//                 break;
                
//             case SpawnMode.FromTilemap:
//                 initialized = InitializeFromTilemap();
//                 break;
                
//             case SpawnMode.FromManualPoints:
//                 initialized = InitializeFromManualPoints();
//                 break;
//         }
        
//         if (!initialized)
//         {
//             Debug.LogWarning($"[EnemySpawner] Failed to initialize spawn positions (mode: {spawnMode}). Nothing spawned.");
//             yield break;
//         }

//         if (clearPreviousSpawns)
//         {
//             ClearSpawned();
//         }

//         var player = FindFirstObjectByType<PlayerController>();
//         Transform playerTransform = player != null ? player.transform : null;
//         if (playerTransform == null)
//         {
//             var character = FindFirstObjectByType<Character>();
//             playerTransform = character != null ? character.transform : null;
//         }

//         foreach (var entry in enemiesToSpawn)
//         {
//             if (entry.prefab == null || entry.count <= 0) continue;

//             for (int i = 0; i < entry.count; i++)
//             {
//                 if (TryFindSpawnPoint(playerTransform, out var spawnPos))
//                 {
//                     var instance = Instantiate(entry.prefab, spawnPos, Quaternion.identity);
//                     _spawned.Add(instance);
//                 }
//             }
//         }
//     }
    
//     private IEnumerator InitializeFromDungeonGenerator()
//     {
//         if (_generator == null)
//         {
//             _generator = FindFirstObjectByType<RoomFirstDungeonGenerator>();
//         }

//         // Wait until dungeon is generated.
//         float timeoutAt = Time.time + 5f;
//         while ((_generator == null || _generator.floorPositions == null || _generator.floorPositions.Count == 0) && Time.time < timeoutAt)
//         {
//             yield return null;
//         }

//         if (_generator == null || _generator.floorPositions == null || _generator.floorPositions.Count == 0)
//         {
//             yield return false;
//         }
//         else
//         {
//             _floorPositions = _generator.floorPositions;
//             _floorTilemap = _generator.TilemapVisualizer != null ? _generator.TilemapVisualizer.FloorTilemap : _floorTilemap;
//             yield return true;
//         }
//     }
    
//     private bool InitializeFromTilemap()
//     {
//         if (manualFloorTilemap == null)
//         {
//             Debug.LogError("[EnemySpawner] Manual floor tilemap not assigned!");
//             return false;
//         }
        
//         _floorTilemap = manualFloorTilemap;
//         _floorPositions = new HashSet<Vector2Int>();
        
//         BoundsInt bounds = manualFloorTilemap.cellBounds;
//         foreach (var pos in bounds.allPositionsWithin)
//         {
//             if (manualFloorTilemap.HasTile(pos))
//             {
//                 _floorPositions.Add(new Vector2Int(pos.x, pos.y));
//             }
//         }
        
//         Debug.Log($"[EnemySpawner] Found {_floorPositions.Count} floor tiles from tilemap.");
//         return _floorPositions.Count > 0;
//     }
    
//     private bool InitializeFromManualPoints()
//     {
//         if (manualSpawnPoints == null || manualSpawnPoints.Length == 0)
//         {
//             Debug.LogError("[EnemySpawner] No manual spawn points assigned!");
//             return false;
//         }
        
//         // For manual points, we'll handle spawning differently
//         _floorPositions = new HashSet<Vector2Int>();
//         foreach (var point in manualSpawnPoints)
//         {
//             if (point != null)
//             {
//                 _floorPositions.Add(new Vector2Int(
//                     Mathf.RoundToInt(point.position.x),
//                     Mathf.RoundToInt(point.position.y)
//                 ));
//             }
//         }
        
//         Debug.Log($"[EnemySpawner] Using {_floorPositions.Count} manual spawn points.");
//         return _floorPositions.Count > 0;
//     }

//     private bool TryFindSpawnPoint(Transform player, out Vector3 worldPoint)
//     {
//         worldPoint = default;

//         if (_floorPositions == null || _floorPositions.Count == 0) return false;
        
//         int total = _floorPositions.Count;

//         float minPlayerSqr = minDistanceFromPlayer * minDistanceFromPlayer;
//         float minEnemySqr = minDistanceBetweenEnemies * minDistanceBetweenEnemies;

//         for (int attempt = 0; attempt < attemptsPerEnemy; attempt++)
//         {
//             Vector2Int cell = PickRandomFloorCell(total);
//             Vector3 candidate = GetCellCenterWorld(cell);
//             candidate.z = zPosition;

//             if (player != null)
//             {
//                 if ((candidate - player.position).sqrMagnitude < minPlayerSqr)
//                 {
//                     continue;
//                 }
//             }

//             bool tooCloseToOtherEnemy = false;
//             for (int i = 0; i < _spawned.Count; i++)
//             {
//                 if (_spawned[i] == null) continue;
//                 if ((candidate - _spawned[i].transform.position).sqrMagnitude < minEnemySqr)
//                 {
//                     tooCloseToOtherEnemy = true;
//                     break;
//                 }
//             }
//             if (tooCloseToOtherEnemy) continue;

//             // Check for blocked position.
//             if (spawnCheckRadius > 0f)
//             {
//                 var hit = Physics2D.OverlapCircle(candidate, spawnCheckRadius, spawnBlockMask);
//                 if (hit != null) continue;
//             }

//             worldPoint = candidate;
//             return true;
//         }

//         return false;
//     }

//     private Vector2Int PickRandomFloorCell(int total)
//     {
//         int targetIndex = UnityEngine.Random.Range(0, total);
//         int idx = 0;
//         foreach (var cell in _floorPositions)
//         {
//             if (idx == targetIndex) return cell;
//             idx++;
//         }

//         // Fallback (shouldn't happen).
//         foreach (var cell in _floorPositions)
//         {
//             return cell;
//         }
//         return Vector2Int.zero;
//     }

//     private Vector3 GetCellCenterWorld(Vector2Int cell)
//     {
//         if (_floorTilemap != null)
//         {
//             Vector3Int tileCell = new Vector3Int(cell.x, cell.y, 0);
//             Vector3 cellWorld = _floorTilemap.CellToWorld(tileCell);
//             Vector3 size = _floorTilemap.cellSize;
//             return cellWorld + new Vector3(size.x / 2f, size.y / 2f, 0f);
//         }

//         // Fallback: interpret cell coords as world coords.
//         return new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f);
//     }

// #if UNITY_EDITOR
//     private void OnDrawGizmosSelected()
//     {
//         Gizmos.color = Color.cyan;
//         Gizmos.DrawWireSphere(transform.position, spawnCheckRadius);
//     }
// #endif
// }
