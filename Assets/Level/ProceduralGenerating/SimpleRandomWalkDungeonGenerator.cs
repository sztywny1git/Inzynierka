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
        // 1. Definiowanie obszaru startowego
        HashSet<Vector2Int> startPositions = new HashSet<Vector2Int>();
        int halfSize = areaSize / 2;
        
        // Iteracja przez prostokątny obszar wokół centerPosition
        for (int x = -halfSize; x <= halfSize; x++)
        {
            for (int y = -halfSize; y <= halfSize; y++)
            {
                startPositions.Add(new Vector2Int(centerPosition.x + x, centerPosition.y + y));
            }
        }

        // 2. Wykonywanie losowych przejść (random walk)
        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
        
        // Zaczynamy od dodania wszystkich pozycji startowych do podłogi
        floorPositions.UnionWith(startPositions);

        // Używamy wszystkich pozycji startowych do rozpoczęcia pierwszej iteracji
        List<Vector2Int> initialCurrentPositions = startPositions.ToList();
        
        for (int i = 0; i < parameters.iterations; i++)
        {
            // Wybieramy losową pozycję startową z obszaru lub z wcześniej wygenerowanych kafelków
            Vector2Int currentPosition;
            if (i < initialCurrentPositions.Count)
            {
                // W pierwszych iteracjach startujemy z pozycji wewnątrz obszaru
                currentPosition = initialCurrentPositions[i];
            }
            else
            {
                // Po wyczerpaniu pozycji startowych lub gdy parametr jest włączony, losujemy z istniejącej podłogi
                if (parameters.startRandomlyEachIteration || initialCurrentPositions.Count == 0)
                {
                    currentPosition = floorPositions.ElementAt(Random.Range(0, floorPositions.Count));
                }
                else
                {
                    // Zapobieganie błędom, jeśli areaSize jest duże, a iterations małe
                    break;
                }
            }
            
            // Generowanie ścieżki i dodawanie jej do podłogi
            var path = ProceduralGenerationAlgorithms.SimpleRandomWalk(currentPosition, parameters.walkLength);
            floorPositions.UnionWith(path);
        }

        return floorPositions;
    }
}
