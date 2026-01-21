using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class BossRoomGenerator : SimpleRandomWalkDungeonGenerator{

    [SerializeField]
    private BossRoomSpawner bossRoomSpawner;
    private Vector2Int _bossSpawnPosition;
    private Vector2Int _playerSpawnPosition;        
    protected override void RunProceduralGeneration()
    {
        int bossRoomAreaSize = 15;
        HashSet<Vector2Int> floorPostions = RunRandomWalkFromArea(randomWalkParameters, startPosition, bossRoomAreaSize); 
        
        tilemapVisualizer.Clear();
        tilemapVisualizer.PaintFloorTiles(floorPostions);

        // Ustalenie pozycji spawnu
        var spawnPoints = GetBossAndPlayerSpawnPoints(floorPostions, startPosition, bossRoomAreaSize);
        _bossSpawnPosition = spawnPoints.Item1;
        _playerSpawnPosition = spawnPoints.Item2;

        // Wizualizacja ścian
        WallGenerator.CreateWalls(floorPostions,tilemapVisualizer);
        
        // TUTAJ W TYM MIEJSCU MOŻESZ WYWOŁAĆ KOD DO WIZUALIZACJI SPAWNU 
        // np. tilemapVisualizer.PaintSpecialTile(_playerSpawnPosition, TileType.PlayerSpawn);
        // oraz tilemapVisualizer.PaintSpecialTile(_bossSpawnPosition, TileType.BossSpawn);

        if (bossRoomSpawner != null)
        {
            bossRoomSpawner.SpawnElements(floorPostions, startPosition, bossRoomAreaSize);
        }
        else
        {
            Debug.LogError("BossRoomSpawner jest null");
        }
    }

    public Tuple<Vector2Int, Vector2Int> GetBossAndPlayerSpawnPoints(
        HashSet<Vector2Int> floorPositions, 
        Vector2Int centerPosition, 
        int areaSize)
    {
        // 1. Pozycja Bossa (Center)
        // Boss jest zawsze w centrum, gwarantowanym przez metodę generacji.
        Vector2Int bossSpawnPosition = centerPosition;

        // 2. Pozycja Gracza (Edge)
        
        // Zaczynamy szukać pozycji na krawędzi w kierunku -Y (na dole)
        Vector2Int playerSpawnPosition = Vector2Int.zero;
        int maxDistance = 0; // Największa odległość od centrum w dół
        
        // Określamy minimalną odległość, aby nie spawnować gracza w centralnym polu (areaSize)
        // Chcemy, aby gracz był poza obszarem startowym.
        int minSpawnDistance = areaSize / 2 + 3; // Np. 3 kafelki poza krawędzią centralnego pola.

        foreach (var pos in floorPositions)
        {
            // Sprawdzamy tylko kafelki na podłodze
            // Szukamy kafelka, który jest najbardziej na dole (najmniejsza wartość Y)
            // i jest wystarczająco daleko od centrum w osi Y.
            
            int distanceY = centerPosition.y - pos.y;
            
            if (distanceY > maxDistance && distanceY >= minSpawnDistance)
            {
                // Dodatkowy warunek, aby upewnić się, że to nie jest tylko cienki korytarz:
                // Sprawdzamy, czy w pobliżu (w osi X) są inne kafelki.
                // Uproszczenie: Upewniamy się, że pozycja jest blisko osi X centrum.
                if (Mathf.Abs(pos.x - centerPosition.x) < areaSize / 4) 
                {
                    maxDistance = distanceY;
                    playerSpawnPosition = pos;
                }
            }
        }
        
        // Zabezpieczenie: Jeśli nie znaleziono optymalnej pozycji, użyj pozycji na krawędzi centralnego obszaru
        if (playerSpawnPosition == Vector2Int.zero)
        {
            playerSpawnPosition = centerPosition + new Vector2Int(0, -minSpawnDistance);
        }

        return new Tuple<Vector2Int, Vector2Int>(bossSpawnPosition, playerSpawnPosition);
    }

}