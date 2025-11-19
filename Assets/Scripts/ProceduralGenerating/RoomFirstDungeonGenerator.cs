using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomFirstDungeonGenerator : SimpleRandomWalkDungeonGenerator
{
    [SerializeField]
    private int minRoomWidth = 4, minRoomHeight = 4;
    [SerializeField]
    private int dungeonWidth = 20, dungeonHeight = 20;
    [SerializeField]
    [Range(0, 10)]
    private int offset = 1;
    [SerializeField]
    private bool randomWalkRooms = false;

    // Add fields to track rooms
    private BoundsInt bossRoom;
    private BoundsInt playerRoom;
    private List<BoundsInt> allRooms;

    protected override void RunProceduralGeneration()
    {
        CreateRooms();
    }

    private void CreateRooms()
    {
        allRooms = ProceduralGenerationAlgorithms.BinarySpacePartitioning(new BoundsInt((Vector3Int)startPosition,
            new Vector3Int(dungeonWidth, dungeonHeight, 0)), minRoomWidth, minRoomHeight);

        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();

        if (randomWalkRooms)
        {
            floor = CreateRoomsRandomly(allRooms);
        }
        else
        {
            floor = CreateSimpleRooms(allRooms);
        }

        List<Vector2Int> roomCenters = FindCentersOfRooms(allRooms);

        HashSet<Vector2Int> corridors = ConnectRooms(roomCenters);
        floor.UnionWith(corridors);

        // Find and mark the boss room and player room
        FindSpecialRooms(allRooms, floor);

        tilemapVisualizer.PaintFloorTiles(floor);
        WallGenerator.CreateWalls(floor, tilemapVisualizer);

        // Spawn player and boss
        SpawnPlayerAndBoss();
    }

    private void FindSpecialRooms(List<BoundsInt> roomsList, HashSet<Vector2Int> floor)
    {
        if (roomsList.Count == 0) return;

        // Find player room (closest to startPosition)
        playerRoom = FindClosestRoomToPosition(roomsList, startPosition);

        // Find boss room (farthest from player room)
        bossRoom = FindFarthestRoomFromPlayer(roomsList, floor, playerRoom);

        Debug.Log($"Player room center: {GetRoomCenter(playerRoom)}");
        Debug.Log($"Boss room center: {GetRoomCenter(bossRoom)}");
    }

    private BoundsInt FindClosestRoomToPosition(List<BoundsInt> roomsList, Vector2Int position)
    {
        BoundsInt closestRoom = roomsList[0];
        float closestDistance = Vector2.Distance(position, GetRoomCenter(closestRoom));

        foreach (var room in roomsList)
        {
            float distance = Vector2.Distance(position, GetRoomCenter(room));
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestRoom = room;
            }
        }

        return closestRoom;
    }

    private BoundsInt FindFarthestRoomFromPlayer(List<BoundsInt> roomsList, HashSet<Vector2Int> floor, BoundsInt playerRoom)
    {
        // Create a graph from the floor tiles
        Dictionary<Vector2Int, List<Vector2Int>> graph = CreateGraph(floor);

        // Use BFS to find the farthest point from the player room
        Vector2Int startPoint = GetRoomCenter(playerRoom);
        Vector2Int farthestPoint = FindFarthestPointBFS(graph, startPoint);

        // Find which room contains the farthest point
        return FindRoomContainingPoint(roomsList, farthestPoint);
    }

    private void SpawnPlayerAndBoss()
    {
        if (spawner != null)
        {
            // Spawn player in the center of the closest room
            Vector2Int playerSpawnPosition = GetRoomCenter(playerRoom);
            spawner.SpawnPlayer(playerSpawnPosition);

            // Spawn boss in the center of the boss room
            Vector2Int bossSpawnPosition = GetRoomCenter(bossRoom);
            spawner.SpawnBoss(bossSpawnPosition);
        }
        else
        {
            Debug.LogWarning("Spawner reference not set in RoomFirstDungeonGenerator");
        }
    }

    // Helper method to convert room center to Vector2Int
    private Vector2Int GetRoomCenter(BoundsInt room)
    {
        Vector3 center3 = room.center;
        return new Vector2Int(Mathf.RoundToInt(center3.x), Mathf.RoundToInt(center3.y));
    }

    // Graph and BFS methods
    private Dictionary<Vector2Int, List<Vector2Int>> CreateGraph(HashSet<Vector2Int> floor)
    {
        Dictionary<Vector2Int, List<Vector2Int>> graph = new Dictionary<Vector2Int, List<Vector2Int>>();

        Vector2Int[] directions = {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        foreach (Vector2Int position in floor)
        {
            List<Vector2Int> neighbors = new List<Vector2Int>();

            foreach (Vector2Int direction in directions)
            {
                Vector2Int neighbor = position + direction;
                if (floor.Contains(neighbor))
                {
                    neighbors.Add(neighbor);
                }
            }

            graph[position] = neighbors;
        }

        return graph;
    }

    private Vector2Int FindFarthestPointBFS(Dictionary<Vector2Int, List<Vector2Int>> graph, Vector2Int start)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, int> distances = new Dictionary<Vector2Int, int>();

        queue.Enqueue(start);
        distances[start] = 0;

        Vector2Int farthestPoint = start;
        int maxDistance = 0;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (graph.ContainsKey(current))
            {
                foreach (Vector2Int neighbor in graph[current])
                {
                    if (!distances.ContainsKey(neighbor))
                    {
                        distances[neighbor] = distances[current] + 1;
                        queue.Enqueue(neighbor);

                        if (distances[neighbor] > maxDistance)
                        {
                            maxDistance = distances[neighbor];
                            farthestPoint = neighbor;
                        }
                    }
                }
            }
        }

        return farthestPoint;
    }

    private BoundsInt FindRoomContainingPoint(List<BoundsInt> roomsList, Vector2Int point)
    {
        foreach (BoundsInt room in roomsList)
        {
            // Convert room bounds to check if point is inside
            if (point.x >= room.xMin && point.x <= room.xMax &&
                point.y >= room.yMin && point.y <= room.yMax)
            {
                return room;
            }
        }

        return roomsList[0];
    }

    // Rest of your existing methods with corrections
    private static List<Vector2Int> FindCentersOfRooms(List<BoundsInt> roomsList)
    {
        List<Vector2Int> roomCenters = new List<Vector2Int>();
        foreach (var room in roomsList)
        {
            Vector3 center3 = room.center;
            roomCenters.Add(new Vector2Int(Mathf.RoundToInt(center3.x), Mathf.RoundToInt(center3.y)));
        }
        return roomCenters;
    }

    private HashSet<Vector2Int> CreateRoomsRandomly(List<BoundsInt> roomsList)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        for (int i = 0; i < roomsList.Count; i++)
        {
            var roomBounds = roomsList[i];
            var roomCenter = GetRoomCenter(roomBounds);
            var roomFloor = RunRandomWalk(randomWalkParameters, roomCenter);
            foreach (var position in roomFloor)
            {
                if (position.x >= (roomBounds.xMin + offset) && position.x <= (roomBounds.xMax - offset) &&
                    position.y >= (roomBounds.yMin + offset) && position.y <= (roomBounds.yMax - offset))
                {
                    floor.Add(position);
                }
            }
        }
        return floor;
    }

    private HashSet<Vector2Int> ConnectRooms(List<Vector2Int> roomCenters)
    {
        HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();
        var currentRoomCenter = roomCenters[Random.Range(0, roomCenters.Count)];
        roomCenters.Remove(currentRoomCenter);

        while (roomCenters.Count > 0)
        {
            Vector2Int closest = FindClosestPointTo(currentRoomCenter, roomCenters);
            roomCenters.Remove(closest);
            HashSet<Vector2Int> newCorridor = CreateCorridor(currentRoomCenter, closest);
            currentRoomCenter = closest;
            corridors.UnionWith(newCorridor);
        }
        return corridors;
    }

    private HashSet<Vector2Int> CreateCorridor(Vector2Int currentRoomCenter, Vector2Int destination)
    {
        HashSet<Vector2Int> corridor = new HashSet<Vector2Int>();
        var position = currentRoomCenter;
        corridor.Add(position);
        while (position.y != destination.y)
        {
            if (destination.y > position.y)
            {
                position += Vector2Int.up;
            }
            else if (destination.y < position.y)
            {
                position += Vector2Int.down;
            }
            corridor.Add(position);
        }
        while (position.x != destination.x)
        {
            if (destination.x > position.x)
            {
                position += Vector2Int.right;
            }
            else if (destination.x < position.x)
            {
                position += Vector2Int.left;
            }
            corridor.Add(position);
        }
        return corridor;
    }

    private Vector2Int FindClosestPointTo(Vector2Int currentRoomCenter, List<Vector2Int> roomCenters)
    {
        Vector2Int closest = Vector2Int.zero;
        float distance = float.MaxValue;
        foreach (var position in roomCenters)
        {
            float currentDistance = Vector2.Distance(position, currentRoomCenter);
            if (currentDistance < distance)
            {
                distance = currentDistance;
                closest = position;
            }
        }
        return closest;
    }

    private HashSet<Vector2Int> CreateSimpleRooms(List<BoundsInt> roomsList)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        foreach (var room in roomsList)
        {
            for (int col = offset; col < room.size.x - offset; col++)
            {
                for (int row = offset; row < room.size.y - offset; row++)
                {
                    Vector2Int position = (Vector2Int)room.min + new Vector2Int(col, row);
                    floor.Add(position);
                }
            }
        }
        return floor;
    }
}