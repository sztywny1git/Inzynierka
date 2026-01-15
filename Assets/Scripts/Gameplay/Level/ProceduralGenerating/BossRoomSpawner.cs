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
    private float propSpawnDensity = 0.05f; // Gęstość spawnowania rekwizytów (np. 5% wolnych kafelków)

    // Publiczne właściwości do odczytu pozycji spawnu (dla Game Managera)
    public Vector2Int PlayerSpawnPosition { get; private set; }
    public Vector2Int BossSpawnPosition { get; private set; }

    // Wywołaj tę metodę po wygenerowaniu podłogi i ścian
    public void SpawnElements(HashSet<Vector2Int> floorPositions, Vector2Int centerPosition, int areaSize)
    {
        if (dungeonGenerator == null)
        {
            Debug.LogError("Brak referencji do Dungeon Generatora!");
            return;
        }

        // 1. POBIERZ I ZAPISZ POZYCJE SPAWNU
        var spawnPoints = dungeonGenerator.GetBossAndPlayerSpawnPoints(floorPositions, centerPosition, areaSize);
        BossSpawnPosition = spawnPoints.Item1;
        PlayerSpawnPosition = spawnPoints.Item2;

        // 2. SPAWN BOSS
        SpawnBoss(BossSpawnPosition);

        // 3. SPAWN GRACZA (tylko wirtualnie - to Game Manager go tam przeniesie)
        // Możesz tutaj spawnować obiekt teleportu powrotnego, jeśli jest zdefiniowany
        SpawnPlayerTeleport(PlayerSpawnPosition);

        // 4. SPAWN REKWIZYTÓW (PROPS)
        SpawnProps(floorPositions, centerPosition);
    }

    private void SpawnBoss(Vector2Int position)
    {
        if (spawnData.bossPrefab != null)
        {
            // Konwersja Vector2Int na Vector3 (dla Unity)
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
            // Teleport powinien być umieszczony na pozycji gracza
            Vector3 spawnPos = new Vector3(position.x + 0.5f, position.y + 0.5f, 0); 
            Instantiate(spawnData.exitTeleportPrefab, spawnPos, Quaternion.identity, transform);
            Debug.Log($"Spawned Exit Teleport at: {position}");
        }
    }

    private void SpawnProps(HashSet<Vector2Int> floorPositions, Vector2Int centerPosition)
    {
        if (spawnData.propPrefabs == null || spawnData.propPrefabs.Count == 0) return;

        // Obszar centralny (gdzie chcemy zostawić najwięcej miejsca)
        int safeZoneSize = 7; // Przykładowo, kwadrat 7x7 wokół centrum musi być pusty
        int halfSafeZone = safeZoneSize / 2;

        foreach (var pos in floorPositions)
        {
            // A. Pomiń obszar bezpieczny wokół Bossa
            if (pos.x >= centerPosition.x - halfSafeZone && 
                pos.x <= centerPosition.x + halfSafeZone &&
                pos.y >= centerPosition.y - halfSafeZone && 
                pos.y <= centerPosition.y + halfSafeZone)
            {
                continue; 
            }

            // B. Sprawdź losową gęstość
            if (Random.value < propSpawnDensity)
            {
                // C. Sprawdź, czy obok nie ma już rekwizytu (opcjonalnie, dla lepszego rozłożenia)
                // Możesz dodać logikę sprawdzania sąsiadów, aby uniknąć nakładania się

                // D. Spawnuj rekwizyt
                GameObject propToSpawn = spawnData.propPrefabs[Random.Range(0, spawnData.propPrefabs.Count)];
                Vector3 spawnPos = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0); // Centrum kafelka
                Instantiate(propToSpawn, spawnPos, Quaternion.identity, transform);
            }
        }
    }
}