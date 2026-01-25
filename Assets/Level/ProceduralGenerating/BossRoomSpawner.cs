using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BossRoomSpawner : MonoBehaviour
{
    [Header("Generator Reference")]
    [SerializeField]
    private BossRoomGenerator dungeonGenerator; 

    [Header("Spawn Data")]
    [SerializeField]
    private BossRoomData spawnData;
    
    [Range(0, 1)]
    [SerializeField]
    private float propSpawnDensity = 0.05f;

    public Vector2Int PlayerSpawnPosition { get; private set; }
    public Vector2Int BossSpawnPosition { get; private set; }

    public void SpawnElements(HashSet<Vector2Int> floorPositions, Vector2Int centerPosition, int areaSize)
    {
        if (dungeonGenerator == null)
        {
            Debug.LogError("Brak referencji do Dungeon Generatora!");
            return;
        }

        var spawnPoints = dungeonGenerator.GetBossAndPlayerSpawnPoints(floorPositions, centerPosition, areaSize);
        BossSpawnPosition = spawnPoints.Item1;
        PlayerSpawnPosition = spawnPoints.Item2;

        SpawnBoss(BossSpawnPosition);

        SpawnPlayerTeleport(PlayerSpawnPosition);

        SpawnProps(floorPositions, centerPosition);
    }

    private void SpawnBoss(Vector2Int position)
    {
        if (spawnData.bossPrefab != null)
        {
            Vector3 spawnPos = new Vector3(position.x + 0.5f, position.y + 0.5f, 0); 
            Instantiate(spawnData.bossPrefab, spawnPos, Quaternion.identity, transform);
            Debug.Log($"Spawned Boss at: {position}");
        }
        else
        {
            Debug.LogWarning("Brak zdefiniowanego Boss Prefabu w BossRoomData.");
        }
    }

    private void SpawnPlayerTeleport(Vector2Int position)
    {
        if (spawnData.exitTeleportPrefab != null)
        {
            Vector3 spawnPos = new Vector3(position.x + 0.5f, position.y + 0.5f, 0); 
            Instantiate(spawnData.exitTeleportPrefab, spawnPos, Quaternion.identity, transform);
            Debug.Log($"Spawned Exit Teleport at: {position}");
        }
    }

    private void SpawnProps(HashSet<Vector2Int> floorPositions, Vector2Int centerPosition)
    {
        if (spawnData.propPrefabs == null || spawnData.propPrefabs.Count == 0) return;

        int safeZoneSize = 7; 
        int halfSafeZone = safeZoneSize / 2;

        foreach (var pos in floorPositions)
        {
            if (pos.x >= centerPosition.x - halfSafeZone && 
                pos.x <= centerPosition.x + halfSafeZone &&
                pos.y >= centerPosition.y - halfSafeZone && 
                pos.y <= centerPosition.y + halfSafeZone)
            {
                continue; 
            }

            if (Random.value < propSpawnDensity)
            {
                GameObject propToSpawn = spawnData.propPrefabs[Random.Range(0, spawnData.propPrefabs.Count)];
                Vector3 spawnPos = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0);
                Instantiate(propToSpawn, spawnPos, Quaternion.identity, transform);
            }
        }
    }
}