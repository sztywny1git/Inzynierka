using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemySpawner : MonoBehaviour
{
    [Serializable]
    public struct SpawnEntry
    {
        public GameObject prefab;
        [Min(0)] public int count;
    }

    [Header("What to spawn")]
    [SerializeField] private List<SpawnEntry> enemiesToSpawn = new List<SpawnEntry>();

    [Header("When")]
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private float startDelaySeconds = 0.25f;

    [Header("Placement")]
    [SerializeField] private float minDistanceFromPlayer = 6f;
    [SerializeField] private float minDistanceBetweenEnemies = 1.0f;
    [SerializeField] private float spawnCheckRadius = 0.35f;
    [SerializeField] private LayerMask spawnBlockMask = ~0;
    [SerializeField] private float zPosition = 0f;

    [Header("Attempts")]
    [SerializeField] private int attemptsPerEnemy = 30;

    [Header("Housekeeping")]
    [SerializeField] private bool clearPreviousSpawns = true;

    private RoomFirstDungeonGenerator _generator;
    private Tilemap _floorTilemap;
    private readonly List<GameObject> _spawned = new List<GameObject>(128);

    private void Awake()
    {
        _generator = FindFirstObjectByType<RoomFirstDungeonGenerator>();
        _floorTilemap = _generator != null && _generator.TilemapVisualizer != null
            ? _generator.TilemapVisualizer.FloorTilemap
            : null;
    }

    private void Start()
    {
        if (spawnOnStart)
        {
            StartCoroutine(SpawnRoutine());
        }
    }

    public void SpawnNow()
    {
        StartCoroutine(SpawnRoutine());
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

    private IEnumerator SpawnRoutine()
    {
        if (startDelaySeconds > 0f)
        {
            yield return new WaitForSeconds(startDelaySeconds);
        }

        if (_generator == null)
        {
            _generator = FindFirstObjectByType<RoomFirstDungeonGenerator>();
        }

        // Wait until dungeon is generated.
        float timeoutAt = Time.time + 5f;
        while ((_generator == null || _generator.floorPositions == null || _generator.floorPositions.Count == 0) && Time.time < timeoutAt)
        {
            yield return null;
        }

        if (_generator == null || _generator.floorPositions == null || _generator.floorPositions.Count == 0)
        {
            Debug.LogWarning("[EnemySpawner] No dungeon floorPositions found; nothing spawned.");
            yield break;
        }

        if (clearPreviousSpawns)
        {
            ClearSpawned();
        }

        var player = FindFirstObjectByType<PlayerController>();
        Transform playerTransform = player != null ? player.transform : null;
        if (playerTransform == null)
        {
            var character = FindFirstObjectByType<Character>();
            playerTransform = character != null ? character.transform : null;
        }

        _floorTilemap = _generator.TilemapVisualizer != null ? _generator.TilemapVisualizer.FloorTilemap : _floorTilemap;

        foreach (var entry in enemiesToSpawn)
        {
            if (entry.prefab == null || entry.count <= 0) continue;

            for (int i = 0; i < entry.count; i++)
            {
                if (TryFindSpawnPoint(playerTransform, out var spawnPos))
                {
                    var instance = Instantiate(entry.prefab, spawnPos, Quaternion.identity);
                    _spawned.Add(instance);
                }
            }
        }
    }

    private bool TryFindSpawnPoint(Transform player, out Vector3 worldPoint)
    {
        worldPoint = default;

        // Copy floor positions to a scratch list only once per call for randomness.
        // floorPositions is a HashSet, so we sample by iterating a random index.
        int total = _generator.floorPositions.Count;
        if (total == 0) return false;

        float minPlayerSqr = minDistanceFromPlayer * minDistanceFromPlayer;
        float minEnemySqr = minDistanceBetweenEnemies * minDistanceBetweenEnemies;

        for (int attempt = 0; attempt < attemptsPerEnemy; attempt++)
        {
            Vector2Int cell = PickRandomFloorCell(total);
            Vector3 candidate = GetCellCenterWorld(cell);
            candidate.z = zPosition;

            if (player != null)
            {
                if ((candidate - player.position).sqrMagnitude < minPlayerSqr)
                {
                    continue;
                }
            }

            bool tooCloseToOtherEnemy = false;
            for (int i = 0; i < _spawned.Count; i++)
            {
                if (_spawned[i] == null) continue;
                if ((candidate - _spawned[i].transform.position).sqrMagnitude < minEnemySqr)
                {
                    tooCloseToOtherEnemy = true;
                    break;
                }
            }
            if (tooCloseToOtherEnemy) continue;

            // Check for blocked position.
            if (spawnCheckRadius > 0f)
            {
                var hit = Physics2D.OverlapCircle(candidate, spawnCheckRadius, spawnBlockMask);
                if (hit != null) continue;
            }

            worldPoint = candidate;
            return true;
        }

        return false;
    }

    private Vector2Int PickRandomFloorCell(int total)
    {
        int targetIndex = UnityEngine.Random.Range(0, total);
        int idx = 0;
        foreach (var cell in _generator.floorPositions)
        {
            if (idx == targetIndex) return cell;
            idx++;
        }

        // Fallback (shouldn't happen).
        foreach (var cell in _generator.floorPositions)
        {
            return cell;
        }
        return Vector2Int.zero;
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

        // Fallback: interpret cell coords as world coords.
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
