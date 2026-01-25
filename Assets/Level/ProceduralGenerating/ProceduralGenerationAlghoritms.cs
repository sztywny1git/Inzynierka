using System.Collections.Generic;
using UnityEngine;

public static class ProceduralGenerationAlgorithms
{
    public static HashSet<Vector2Int> SimpleRandomWalk(Vector2Int startPosition, int walkLength)
    {
        HashSet<Vector2Int> path = new HashSet<Vector2Int>();

        path.Add(startPosition);
        var previousPosition = startPosition;

        for(int i = 0; i < walkLength; i++)
        {
            var newPosition = previousPosition + Direction2D.GetRandomCardinalDirection(); 
            path.Add(newPosition);
            previousPosition = newPosition;
        }
        return path;
    }
    public static List<Vector2Int> RandomWalkCorridor(Vector2Int startPosition, int corridorLength)
    {
        List<Vector2Int> corridor = new List<Vector2Int>();
        var direction = Direction2D.GetRandomCardinalDirection();
        var currentPosition = startPosition;
        corridor.Add(currentPosition);

        for(int i = 0;i < corridorLength; i++)
        {
            currentPosition += direction;
            corridor.Add(currentPosition);
        }
        return corridor;
    }

    public static List<BoundsInt> BinarySpacePartitioning(BoundsInt spaceToSplit, int minWidth, int minHeight)
    {
        Queue<BoundsInt> roomsQueue = new Queue<BoundsInt>();
        List<BoundsInt> roomsList = new List<BoundsInt>();
        roomsQueue.Enqueue(spaceToSplit);
        while(roomsQueue.Count > 0)
        {
            var room = roomsQueue.Dequeue();
            if(room.size.y >= minHeight && room.size.x >= minWidth)
            {
                if (Random.value < 0.5f)
                {
                    if(room.size.y >= minHeight * 2)
                    {
                        SplitHorizontally(minWidth, roomsQueue, room);
                    }else if(room.size.x >= minWidth * 2)
                    {
                        SplitVertically( minHeight, roomsQueue, room);
                    }else if(room.size.x >= minWidth && room.size.y >= minHeight)
                    {
                        roomsList.Add(room);
                    }
                }
                else
                {                   
                    if (room.size.x >= minWidth * 2)
                    {
                        SplitVertically(minWidth, roomsQueue, room);
                    }
                    else if (room.size.y >= minHeight * 2)
                    {
                        SplitHorizontally(minHeight, roomsQueue, room);
                    }
                    else if (room.size.x >= minWidth && room.size.y >= minHeight)
                    {
                        roomsList.Add(room);
                    }
                }
            }
        }
        return roomsList;
    }

    private static void SplitVertically(int minWidth, Queue<BoundsInt> roomsQueue, BoundsInt room)
    {
        var xSplit = Random.Range(1, room.size.x);
        BoundsInt room1 = new BoundsInt(room.min,new Vector3Int(xSplit, room.size.y, room.size.z));
        BoundsInt room2 = new BoundsInt(new Vector3Int(room.min.x + xSplit, room.min.y, room.min.z),
            new Vector3Int(room.size.x - xSplit , room.size.y, room.size.z));
        roomsQueue.Enqueue(room1);
        roomsQueue.Enqueue(room2);
    }

    private static void SplitHorizontally(int minHeight, Queue<BoundsInt> roomsQueue, BoundsInt room)
    {
        var ySplit = Random.Range(1, room.size.y);
        BoundsInt room1 = new BoundsInt(room.min, new Vector3Int(room.size.x, ySplit, room.size.z));
        BoundsInt room2 =(new BoundsInt(new Vector3Int(room.min.x, room.min.y + ySplit, room.min.z),
            new Vector3Int(room.size.x,room.size.y - ySplit, room.size.z)));
        roomsQueue.Enqueue(room1);
        roomsQueue.Enqueue(room2);
    }

    public static HashSet<Vector2Int> CellularAutomataSmoothing(HashSet<Vector2Int> currentFloor, int iterations)
    {
        HashSet<Vector2Int> newFloor = new HashSet<Vector2Int>(currentFloor);

        // Obliczamy granice obszaru, żeby nie iterować w nieskończoność
        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;

        foreach (var pos in currentFloor)
        {
            if (pos.x < minX) minX = pos.x;
            if (pos.x > maxX) maxX = pos.x;
            if (pos.y < minY) minY = pos.y;
            if (pos.y > maxY) maxY = pos.y;
        }

        // Dodajemy margines (padding), żeby algorytm mógł "domknąć" ściany na krawędziach
        int padding = 1;

        for (int i = 0; i < iterations; i++)
        {
            // Tworzymy tymczasowy zbiór dla tej iteracji (zmiany nakładamy po pełnym przebiegu)
            HashSet<Vector2Int> nextIterationFloor = new HashSet<Vector2Int>();

            for (int x = minX - padding; x <= maxX + padding; x++)
            {
                for (int y = minY - padding; y <= maxY + padding; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    int neighborCount = CountNeighbors(pos, newFloor);
                    
                    if (neighborCount > 4)
                    {
                        nextIterationFloor.Add(pos); // Staje się podłogą
                    }
                    else if (neighborCount == 4) 
                    {
                        // Stan bez zmian (jeśli był podłogą, zostaje nią)
                        if (newFloor.Contains(pos))
                        {
                            nextIterationFloor.Add(pos);
                        }
                    }
                    // W przeciwnym razie staje się ścianą (nie dodajemy do floor)
                }
            }
            newFloor = nextIterationFloor;
        }

        return newFloor;
    }

    private static int CountNeighbors(Vector2Int position, HashSet<Vector2Int> floor)
    {
        int count = 0;
        // Sprawdzamy 8 sąsiadów (Sąsiedztwo Moore'a)
        foreach (var dir in Direction2D.eightDirectionList)
        {
            if (floor.Contains(position + dir))
            {
                count++;
            }
        }
        return count;
    }

    public static HashSet<Vector2Int> RemoveDisconnectedIslands(HashSet<Vector2Int> floorPositions)
    {
        List<HashSet<Vector2Int>> islands = new List<HashSet<Vector2Int>>();
        HashSet<Vector2Int> processedTiles = new HashSet<Vector2Int>();

        foreach (var position in floorPositions)
        {
            if (!processedTiles.Contains(position))
            {
                var island = RunBFS(position, floorPositions);
                islands.Add(island);

                foreach (var tile in island)
                {
                    processedTiles.Add(tile);
                }
            }
        }

        if (islands.Count == 0)
            return new HashSet<Vector2Int>();

        HashSet<Vector2Int> largestIsland = islands[0];
        foreach (var island in islands)
        {
            if (island.Count > largestIsland.Count)
            {
                largestIsland = island;
            }
        }

        return largestIsland;
    }

    //Algorytm BFS (Breadth-First Search) znajdujący wszystkie połączone kafelki.
    private static HashSet<Vector2Int> RunBFS(Vector2Int startPos, HashSet<Vector2Int> floorPositions)
    {
        HashSet<Vector2Int> island = new HashSet<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        
        queue.Enqueue(startPos);
        island.Add(startPos);

        while (queue.Count > 0)
        {
            var currentPos = queue.Dequeue();

            foreach (var direction in Direction2D.cardinalDirectionList)
            {
                var neighbor = currentPos + direction;
                
                if (floorPositions.Contains(neighbor) && !island.Contains(neighbor))
                {
                    island.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }
        return island;
    }
    
}

public static class Direction2D
{
    public static List<Vector2Int> cardinalDirectionList = new List<Vector2Int>()
    {
         new Vector2Int(0,1), //GÓRA
         new Vector2Int(1,0), //PRAWO
         new Vector2Int(0,-1), //DÓŁ
         new Vector2Int(-1,0) //LEWO
    };

    public static List<Vector2Int> diagonalDirectionList = new List<Vector2Int>()
    {
         new Vector2Int(1,1), //GÓRA-PRAWO
         new Vector2Int(1,-1), //DÓŁ-PRAWO
         new Vector2Int(-1,-1), //DÓŁ-LEWO
         new Vector2Int(-1,1) // LEWO-GÓRA
    };

    public static List<Vector2Int> eightDirectionList = new List<Vector2Int>()
    {
         new Vector2Int(0,1), //GÓRA
         new Vector2Int(1,1), //GÓRA-PRAWO
         new Vector2Int(1,0), //PRAWO
         new Vector2Int(1,-1), //PRAWO-DÓŁ
         new Vector2Int(0,-1), //DÓŁ
         new Vector2Int(-1,-1), //DÓŁ-LEWO
         new Vector2Int(-1,0), // LEWO
         new Vector2Int(-1,1) // LEWO-GÓRA
    };
    public static Vector2Int GetRandomCardinalDirection()
    {
        return cardinalDirectionList[Random.Range(0,cardinalDirectionList.Count)];
    }

}


