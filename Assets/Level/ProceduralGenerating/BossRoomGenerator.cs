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

        var spawnPoints = GetBossAndPlayerSpawnPoints(floorPostions, startPosition, bossRoomAreaSize);
        _bossSpawnPosition = spawnPoints.Item1;
        _playerSpawnPosition = spawnPoints.Item2;

        WallGenerator.CreateWalls(floorPostions,tilemapVisualizer);

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

        Vector2Int bossSpawnPosition = centerPosition;

        Vector2Int playerSpawnPosition = Vector2Int.zero;
        int maxDistance = 0;
        
        int minSpawnDistance = areaSize / 2 + 3; 

        foreach (var pos in floorPositions)
        {
            
            int distanceY = centerPosition.y - pos.y;
            
            if (distanceY > maxDistance && distanceY >= minSpawnDistance)
            {

                if (Mathf.Abs(pos.x - centerPosition.x) < areaSize / 4) 
                {
                    maxDistance = distanceY;
                    playerSpawnPosition = pos;
                }
            }
        }
        

        if (playerSpawnPosition == Vector2Int.zero)
        {
            playerSpawnPosition = centerPosition + new Vector2Int(0, -minSpawnDistance);
        }

        return new Tuple<Vector2Int, Vector2Int>(bossSpawnPosition, playerSpawnPosition);
    }

}