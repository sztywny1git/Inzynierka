using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class SimpleRandomWalkDungeonGenerator : AbstractDungeonGenerator {

    [SerializeField]
    protected SimpleRandomWalkData randomWalkParameters;


    protected override void RunProceduralGeneration()
    {
        HashSet<Vector2Int> floorPostions = RunRandomWalk(randomWalkParameters, startPosition);
        tilemapVisualizer.Clear();
        tilemapVisualizer.PaintFloorTiles(floorPostions);
        WallGenerator.CreateWalls(floorPostions,tilemapVisualizer);
    }

    protected HashSet<Vector2Int> RunRandomWalk(SimpleRandomWalkData parameters, Vector2Int position)
    {
        var currentPosition = position;
        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
        for(int i = 0; i < parameters.iterations; i++)
        {
            var path = ProceduralGenerationAlgorithms.SimpleRandomWalk(currentPosition, parameters.walkLength);
            floorPositions.UnionWith(path);
            if(parameters.startRandomlyEachIteration)
                currentPosition = floorPositions.ElementAt(Random.Range(0, floorPositions.Count));    
        }
        return floorPositions;
    }
    protected HashSet<Vector2Int> RunRandomWalkFromArea(SimpleRandomWalkData parameters, Vector2Int centerPosition, int areaSize)
    {
        HashSet<Vector2Int> startPositions = new HashSet<Vector2Int>();
        int halfSize = areaSize / 2;
        
        for (int x = -halfSize; x <= halfSize; x++)
        {
            for (int y = -halfSize; y <= halfSize; y++)
            {
                startPositions.Add(new Vector2Int(centerPosition.x + x, centerPosition.y + y));
            }
        }

        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
        
        floorPositions.UnionWith(startPositions);

        List<Vector2Int> initialCurrentPositions = startPositions.ToList();
        
        for (int i = 0; i < parameters.iterations; i++)
        {

            Vector2Int currentPosition;
            if (i < initialCurrentPositions.Count)
            {

                currentPosition = initialCurrentPositions[i];
            }
            else
            {

                if (parameters.startRandomlyEachIteration || initialCurrentPositions.Count == 0)
                {
                    currentPosition = floorPositions.ElementAt(Random.Range(0, floorPositions.Count));
                }
                else
                {

                    break;
                }
            }
            
            var path = ProceduralGenerationAlgorithms.SimpleRandomWalk(currentPosition, parameters.walkLength);
            floorPositions.UnionWith(path);
        }

        return floorPositions;
    }
}
