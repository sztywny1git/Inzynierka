using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public struct RoomPropDataEntry
{
    public RoomType roomType;
    public RoomPropData propData;
}

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

    [Header("Special Room Settings")]
    [SerializeField]
    private int puzzleRoomCount = 2;

    [Header("Room Size Modifiers")]
    [SerializeField] private int bossRoomExpand = 4;
    [SerializeField] private int puzzleRoomExpand = 2;

    [Header("Prop Spawning Settings")]
    [SerializeField] private Vector2 propSpawnOffset = new Vector2(0.5f, 0.5f);
    [SerializeField] private List<RoomPropDataEntry> roomPropDataList = new List<RoomPropDataEntry>();

    [Header("Managers")]
    [SerializeField] private PuzzleManager puzzleManager; 

    [Header("Debug / Wizualizacja")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color fullGraphColor = Color.yellow;
    [SerializeField] private Color mstColor = Color.blue;
    [SerializeField] private Color finalPathColor = Color.red;

    [Header("Cellular Automata (Smoothing)")]
    [SerializeField] private bool useSmoothing = true;
    [SerializeField] [Range(1, 5)] private int smoothingIterations = 1;

    private List<GraphAlgorithms.Edge> debugAllEdges = new List<GraphAlgorithms.Edge>();
    private List<GraphAlgorithms.Edge> debugMSTEdges = new List<GraphAlgorithms.Edge>();
    private List<GraphAlgorithms.Edge> debugFinalEdges = new List<GraphAlgorithms.Edge>();

    private BoundsInt bossRoom;
    private BoundsInt playerRoom;
    public List<BoundsInt> allRooms;
    public List<BoundsInt> MainPathRooms;
    protected HashSet<Vector2Int> corridorPositions = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> WallPositions = new HashSet<Vector2Int>();
    public TilemapVisualizer TilemapVisualizer => tilemapVisualizer;
    private Dictionary<RoomType, RoomPropData> roomPropDataMap;
    private Dictionary<BoundsInt, RoomType> roomTypes = new Dictionary<BoundsInt, RoomType>();

    public void GenerateDungeon()
    {
        RunProceduralGeneration();
    }

    public Vector2Int GetPlayerSpawnPosition()
    {
        return DungeonHelper.GetRoomCenter(playerRoom);
    }

    public Vector2Int GetBossSpawnPosition()
    {
        return DungeonHelper.GetRoomCenter(bossRoom);
    }

    private void InitializePropDataMap()
    {
        roomPropDataMap = new Dictionary<RoomType, RoomPropData>();

        foreach (var entry in roomPropDataList)
        {
            if (entry.propData != null)
            {
                if (roomPropDataMap.ContainsKey(entry.roomType))
                {
                    roomPropDataMap[entry.roomType] = entry.propData;
                }
                else
                {
                    roomPropDataMap.Add(entry.roomType, entry.propData);
                }
            }
        }
    }

    private void FindWalls()
    {
        WallPositions.Clear();
        foreach (var pos in floorPositions)
        {
            foreach (var dir in Direction2D.eightDirectionList)
            {
                Vector2Int neighbor = pos + dir;
                if (!floorPositions.Contains(neighbor))
                {
                    WallPositions.Add(neighbor);
                }
            }
        }
    }

    protected override void RunProceduralGeneration()
    {
        InitializePropDataMap();
        
        if (spawner != null && puzzleManager != null)
        {
            spawner.Initialize(puzzleManager);
        }

        HashSet<Vector2Int> currentFloor = CreateRooms();
        
        floorPositions = currentFloor;

        tilemapVisualizer.Clear();
        tilemapVisualizer.PaintFloorTiles(floorPositions);
        WallGenerator.CreateWalls(floorPositions, tilemapVisualizer);
        
        FindWalls();
        
        FindSpecialRooms(allRooms, floorPositions);

        AssignRoomTypes(allRooms, floorPositions);

        tilemapVisualizer.PaintRoomTypes(roomTypes);

        SpawnRoomContent();
        SpawnPropsInRooms();
    }

    private HashSet<Vector2Int> CreateRooms() 
    {
        allRooms = ProceduralGenerationAlgorithms.BinarySpacePartitioning(new BoundsInt((Vector3Int)startPosition,
            new Vector3Int(dungeonWidth, dungeonHeight, 0)), minRoomWidth, minRoomHeight);

        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();

        ApplyRoomShapeModifications();

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

        corridorPositions = corridors;

        floor.UnionWith(corridors);

        return floor;
    }

    private void FindSpecialRooms(List<BoundsInt> roomsList, HashSet<Vector2Int> floor)
    {
        if (roomsList == null || roomsList.Count == 0) return;

        playerRoom = FindClosestRoomToPosition(roomsList, startPosition);
        bossRoom = FindFarthestRoomFromPlayer(roomsList, floor, playerRoom);
    }

    private BoundsInt FindClosestRoomToPosition(List<BoundsInt> roomsList, Vector2Int position)
    {
        BoundsInt closestRoom = roomsList[0];
        float closestDistance = Vector2.Distance(position, DungeonHelper.GetRoomCenter(closestRoom));

        foreach (var room in roomsList)
        {
            float distance = Vector2.Distance(position, DungeonHelper.GetRoomCenter(room));
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
        Dictionary<Vector2Int, List<Vector2Int>> graph = CreateGraph(floor);
        Vector2Int startPoint = DungeonHelper.GetRoomCenter(playerRoom);
        Vector2Int farthestPoint = FindFarthestPointBFS(graph, startPoint);

        return FindRoomContainingPoint(roomsList, farthestPoint);
    }

    private void SpawnRoomContent()
    {
        foreach (var kvp in roomTypes)
        {
            RoomType type = kvp.Value;
            
            switch (type)
            {
                case RoomType.Puzzle:
                    break;

                case RoomType.Standard:
                    break;

                case RoomType.Start:
                    break;
            }
        }

        if (puzzleManager != null)
        {
            puzzleManager.SpawnPuzzleObjects(new BoundsInt(), spawner); 
        }
    }

    private void SpawnPropsInRooms()
    {
        if (WallPositions.Count == 0)
        {
            Debug.LogError("WallPositions is empty");
        }

        PropWFC wfcSolver = new PropWFC();
        
        HashSet<Vector2Int> globalReserved = new HashSet<Vector2Int>();
        if (spawner.PuzzleManager != null)
        {
            globalReserved.UnionWith(spawner.PuzzleManager.ReservedPuzzlePositions);
        }

        foreach (var room in allRooms)
        {
            if (room.size.x < 4 || room.size.y < 4) continue;

            if (roomTypes.TryGetValue(room, out RoomType type))
            {
                if (!roomPropDataMap.ContainsKey(type))
                {
                    continue;
                }

                RoomPropData data = roomPropDataMap[type];

                Dictionary<PropWFC.PropType, int> currentRoomWeights = data.GetWfcWeights();

                HashSet<Vector2Int> localReserved = new HashSet<Vector2Int>(globalReserved);
                localReserved.UnionWith(corridorPositions);
                
                foreach(var corrPos in corridorPositions)
                    foreach(var dir in Direction2D.eightDirectionList)
                        localReserved.Add(corrPos + dir);

                if (MainPathRooms != null && MainPathRooms.Contains(room))
                {
                    Vector2Int center = DungeonHelper.GetRoomCenter(room);
                    int freeRadius = 2;
                    for (int x = center.x - freeRadius; x <= center.x + freeRadius; x++)
                        for (int y = center.y - freeRadius; y <= center.y + freeRadius; y++)
                            localReserved.Add(new Vector2Int(x, y));
                }

                var wfcResult = wfcSolver.Run(room, floorPositions, WallPositions, localReserved, currentRoomWeights);

                foreach (var kvp in wfcResult)
                {
                    if (!localReserved.Contains(kvp.Key) && kvp.Value != PropWFC.PropType.Empty)
                    {
                        spawner.SpawnWfcProp(kvp.Key, kvp.Value);
                    }
                }
            }
        }
    }

    private bool IsNearCorridor(Vector2Int pos)
    {
        foreach(var dir in Direction2D.eightDirectionList)
        {
            if (corridorPositions.Contains(pos + dir)) return true;
        }
        return false;
    }

    private void SpawnFloorProps(BoundsInt room, RoomPropData propData, List<Vector2Int> safePositions)
    {
        int roomArea = room.size.x * room.size.y;
        int targetCount = Mathf.RoundToInt((float)roomArea / 100f * propData.floorPropDensity);
        
        targetCount = Mathf.Min(targetCount, safePositions.Count);

        for (int i = 0; i < targetCount; i++)
        {
            int index = Random.Range(0, safePositions.Count);
            Vector2Int pos = safePositions[index];
            safePositions.RemoveAt(index);

            RoomPropData.PropEntry propEntry = propData.floorProps[Random.Range(0, propData.floorProps.Count)];
            
            spawner.SpawnProp(propEntry, pos);
        }
    }

    private void SpawnWallProps(BoundsInt room, RoomPropData propData, List<Vector2Int> safePositions)
    {
    }

    private List<Vector2Int> GetSafeFloorPositionsInRoom(BoundsInt room, HashSet<Vector2Int> reserved, HashSet<Vector2Int> floor)
    {
        List<Vector2Int> safePositions = new List<Vector2Int>();
        
        HashSet<Vector2Int> exclusionZone = new HashSet<Vector2Int>();
        
        if (corridorPositions != null)
        {
            exclusionZone.UnionWith(corridorPositions);
            
            foreach (var pos in corridorPositions)
            {
                foreach (var direction in Direction2D.eightDirectionList) 
                {
                    exclusionZone.Add(pos + direction);
                }
            }
        }
        
        if (MainPathRooms != null && MainPathRooms.Contains(room))
        {
            List<BoundsInt> path = MainPathRooms;
            Vector2Int center = DungeonHelper.GetRoomCenter(room);
            
            int freeRadius = 3;
            
            for (int x = center.x - freeRadius; x <= center.x + freeRadius; x++)
            {
                for (int y = center.y - freeRadius; y <= center.y + freeRadius; y++)
                {
                    exclusionZone.Add(new Vector2Int(x, y));
                }
            }
        }

        for (int x = room.xMin + offset; x < room.xMax - offset; x++)
        {
            for (int y = room.yMin + offset; y < room.yMax - offset; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                
                if (floor.Contains(pos) && !reserved.Contains(pos) && !exclusionZone.Contains(pos))
                {
                    safePositions.Add(pos);
                }
            }
        }
        return safePositions;
    }

    private List<Vector2Int> GetSafeWallPositionsInRoom(BoundsInt room, HashSet<Vector2Int> reserved, HashSet<Vector2Int> wall)
    {
        List<Vector2Int> safeWallPositions = new List<Vector2Int>();
        
        for (int x = room.xMin - 1; x <= room.xMax + 1; x++)
        {
            for (int y = room.yMin - 1; y <= room.yMax + 1; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                if (wall.Contains(pos) && !reserved.Contains(pos))
                {
                    safeWallPositions.Add(pos);
                }
            }
        }
        return safeWallPositions;
    }

    private void ScatterObstaclesInRoom(BoundsInt room, HashSet<Vector2Int> floor, int count)
    {
        var availablePositions = floor
            .Where(p => 
                p.x > room.xMin + offset && p.x < room.xMax - offset &&
                p.y > room.yMin + offset && p.y < room.yMax - offset)
            .ToList();

        for (int i = 0; i < count; i++)
        {
            if (availablePositions.Count == 0) break;

            int index = Random.Range(0, availablePositions.Count);
            Vector2Int spawnPos = availablePositions[index];
            availablePositions.RemoveAt(index);
        }
    }

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

    public BoundsInt FindRoomContainingPoint(List<BoundsInt> roomsList, Vector2Int point)
    {
        foreach (BoundsInt room in roomsList)
        {
            if (point.x >= room.xMin && point.x <= room.xMax &&
                point.y >= room.yMin && point.y <= room.yMax)
            {
                return room;
            }
        }

        return roomsList[0];
    }

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
            var roomCenter = DungeonHelper.GetRoomCenter(roomBounds);
            
            var roomFloor = RunRandomWalk(randomWalkParameters, roomCenter);

            if (useSmoothing)
            {
                roomFloor = ProceduralGenerationAlgorithms.CellularAutomataSmoothing(roomFloor, smoothingIterations);
            }

            roomFloor = ProceduralGenerationAlgorithms.RemoveDisconnectedIslands(roomFloor);

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
        
        debugAllEdges.Clear();
        debugMSTEdges.Clear();
        debugFinalEdges.Clear();

        HashSet<GraphAlgorithms.Edge> allEdgesSet = new HashSet<GraphAlgorithms.Edge>();
        for (int i = 0; i < roomCenters.Count; i++)
        {
            for (int j = i + 1; j < roomCenters.Count; j++)
            {
                var edge = new GraphAlgorithms.Edge(roomCenters[i], roomCenters[j]);
                allEdgesSet.Add(edge);
            }
        }
        List<GraphAlgorithms.Edge> allEdgesList = allEdgesSet.ToList();
        
        debugAllEdges = new List<GraphAlgorithms.Edge>(allEdgesList); 

        List<GraphAlgorithms.Edge> mstEdges = GraphAlgorithms.KruskalMST(roomCenters, allEdgesList);
        
        debugMSTEdges = new List<GraphAlgorithms.Edge>(mstEdges);

        List<GraphAlgorithms.Edge> finalEdges = new List<GraphAlgorithms.Edge>(mstEdges);
        
        HashSet<GraphAlgorithms.Edge> existingEdgesSet = new HashSet<GraphAlgorithms.Edge>(mstEdges);
        
        var unusedEdges = allEdgesList.Where(e => !existingEdgesSet.Contains(e)).ToList();
        
        foreach (var edge in unusedEdges)
        {
            if (UnityEngine.Random.value < 0.50f && edge.Distance < 25f) 
            {
                finalEdges.Add(edge);
            }
        }

        debugFinalEdges = new List<GraphAlgorithms.Edge>(finalEdges);

        foreach (var edge in finalEdges)
        {
            HashSet<Vector2Int> newCorridor = CreateCorridor(edge.U, edge.V);
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
            RoomType type = roomTypes.ContainsKey(room)
                ? roomTypes[room]
                : RoomType.Standard;

            HashSet<Vector2Int> layout;

            switch (type)
            {
                case RoomType.Boss:
                    layout = CreateBossArena(room);
                    break;

                case RoomType.Puzzle:
                    layout = CreatePuzzleRoomLayout(room);
                    break;

                default:
                    layout = CreateStandardRoomLayout(room);
                    break;
            }

            floor.UnionWith(layout);
        }

        return floor;
    }

    private void AssignRoomTypes(List<BoundsInt> roomsList, HashSet<Vector2Int> floor)
    {
        roomTypes.Clear();

        if (roomsList == null || roomsList.Count == 0) return;

        roomTypes[playerRoom] = RoomType.Start;

        roomTypes[bossRoom] = RoomType.Boss;

        var graph = CreateGraph(floor);
        var startCenter = DungeonHelper.GetRoomCenter(playerRoom);
        var bossCenter = DungeonHelper.GetRoomCenter(bossRoom);

        List<Vector2Int> mainPathVectors = ReconstructPathBFS(graph, startCenter, bossCenter); 

        List<BoundsInt> mainRooms = new List<BoundsInt>();
        HashSet<BoundsInt> mainRoomsSet = new HashSet<BoundsInt>();
        
        foreach (var pos in mainPathVectors)
        {
            var r = FindRoomContainingPoint(roomsList, pos);
            if (r.size.magnitude > 0 && !mainRoomsSet.Contains(r))
            {
                mainRoomsSet.Add(r);
                mainRooms.Add(r); 
            }
        }
        
        MainPathRooms = mainRooms; 

        var sideRooms = roomsList
            .Where(r => r != playerRoom && r != bossRoom && !mainRoomsSet.Contains(r))
            .ToList();

        if (sideRooms.Count < puzzleRoomCount)
        {
            var extraRooms = mainRooms
                .Where(r => r != playerRoom && r != bossRoom)
                .ToList();

            sideRooms.AddRange(extraRooms);
        }
        
        var chosenPuzzles = sideRooms
            .OrderBy(x => Random.value)
            .Take(puzzleRoomCount)
            .ToList();

        List<(BoundsInt room, bool isKey)> puzzleRoomsWithContext = new List<(BoundsInt room, bool isKey)>();

        foreach (var pr in chosenPuzzles)
        {
            roomTypes[pr] = RoomType.Puzzle;
            
            bool isKeyPuzzle = mainRoomsSet.Contains(pr); 
            
            puzzleRoomsWithContext.Add((pr, isKeyPuzzle));
        }

        if (puzzleManager != null)
        {
            puzzleManager.DungeonGenerator = this; 
            puzzleManager.PreparePuzzles(puzzleRoomsWithContext);
        }

        foreach (var room in roomsList)
        {
            if (!roomTypes.ContainsKey(room))
                roomTypes[room] = RoomType.Standard;
        }
    }

    private List<Vector2Int> ReconstructPathBFS(
        Dictionary<Vector2Int, List<Vector2Int>> graph,
        Vector2Int start,
        Vector2Int goal)
    {
        List<Vector2Int> path = new List<Vector2Int>();

        if (start == goal)
        {
            path.Add(start);
            return path;
        }

        if (graph == null || !graph.ContainsKey(start) || !graph.ContainsKey(goal))
        {
            path.Add(start);
            path.Add(goal);
            return path;
        }

        Dictionary<Vector2Int, Vector2Int> parent = new Dictionary<Vector2Int, Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        queue.Enqueue(start);
        visited.Add(start);

        bool found = false;
        while (queue.Count > 0)
        {
            var curr = queue.Dequeue();
            if (curr == goal)
            {
                found = true;
                break;
            }

            if (!graph.ContainsKey(curr)) continue;

            foreach (var n in graph[curr])
            {
                if (!visited.Contains(n))
                {
                    visited.Add(n);
                    parent[n] = curr;
                    queue.Enqueue(n);
                }
            }
        }

        if (!found)
        {
            path.Add(start);
            path.Add(goal);
            return path;
        }

        var p = goal;
        while (!p.Equals(start))
        {
            path.Add(p);
            if (!parent.ContainsKey(p)) break;
            p = parent[p];
        }

        path.Add(start);
        path.Reverse();
        return path;
    }

    public RoomType GetRoomType(BoundsInt room)
    {
        if (roomTypes.ContainsKey(room)) return roomTypes[room];
        return RoomType.Undefined;
    }

    private void ApplyRoomShapeModifications()
    {
        for (int i = 0; i < allRooms.Count; i++)
        {
            var room = allRooms[i];

            if (!roomTypes.ContainsKey(room))
                continue;

            RoomType type = roomTypes[room];

            switch (type)
            {
                case RoomType.Boss:
                    allRooms[i] = ExpandRoom(room, bossRoomExpand);
                    break;

                case RoomType.Puzzle:
                    allRooms[i] = ExpandRoom(room, puzzleRoomExpand);
                    break;

                case RoomType.Standard:
                    break;
            }
        }
    }

    private BoundsInt ExpandRoom(BoundsInt room, int amount)
    {
        Vector3Int min = room.min - new Vector3Int(amount, amount, 0);
        Vector3Int max = room.max + new Vector3Int(amount, amount, 0);
        Vector3Int size = max - min;
        return new BoundsInt(min, size);
    }

    private HashSet<Vector2Int> CreateBossArena(BoundsInt room)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();

        Vector2Int center = DungeonHelper.GetRoomCenter(room);
        int radius = Mathf.Min(room.size.x, room.size.y) / 2 - 1;

        for (int x = -radius; x <= radius; x++)
            for (int y = -radius; y <= radius; y++)
                if (x * x + y * y <= radius * radius) 
                    floor.Add(center + new Vector2Int(x, y));

        return floor;
    }

    private HashSet<Vector2Int> CreatePuzzleRoomLayout(BoundsInt room)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();

        Vector2Int center = DungeonHelper.GetRoomCenter(room);

        int width = Mathf.Min(room.size.x, 10);
        int height = Mathf.Min(room.size.y, 10);

        int hx = width / 2;
        int hy = height / 2;

        for (int x = -hx; x <= hx; x++)
            for (int y = -hy; y <= hy; y++)
                floor.Add(center + new Vector2Int(x, y));

        return floor;
    }

    private HashSet<Vector2Int> CreateTreasureRoom(BoundsInt room)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        Vector2Int center = DungeonHelper.GetRoomCenter(room);

        for (int x = -2; x <= 2; x++)
            for (int y = -2; y <= 2; y++)
                floor.Add(center + new Vector2Int(x, y));

        return floor;
    }

    private HashSet<Vector2Int> CreateStandardRoomLayout(BoundsInt room)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        for (int x = room.xMin + offset; x < room.xMax - offset; x++)
            for (int y = room.yMin + offset; y < room.yMax - offset; y++)
                floor.Add(new Vector2Int(x, y));

        return floor;
    }

    private void OpenGate(BoundsInt room, Vector2Int gatePosition)
    {
    }

    public bool CheckPuzzlePlate(Vector2Int position)
    {
        if (puzzleManager != null)
        {
            return puzzleManager.CheckPressurePlate(position);
        }
        return false;
    }

    public Vector2Int GetCorridorExitPoint(BoundsInt currentRoom, List<BoundsInt> mainPath)
    {
        if (mainPath == null || mainPath.Count == 0)
        {
            return Vector2Int.zero;
        }

        int currentIndex = mainPath.IndexOf(currentRoom);

        if (currentIndex == -1 || currentIndex >= mainPath.Count - 1)
        {
            return Vector2Int.zero;
        }

        BoundsInt nextRoom = mainPath[currentIndex + 1];

        return FindExitByScanningPerimeter(currentRoom, nextRoom);
    }

    private Vector2Int FindExitByScanningPerimeter(BoundsInt room, BoundsInt targetRoom)
    {
        Vector2 targetCenter = new Vector2(targetRoom.center.x, targetRoom.center.y);
        Vector2Int bestExit = Vector2Int.zero;
        float minDistance = float.MaxValue;

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        for (int x = room.xMin; x < room.xMax; x++)
        {
            for (int y = room.yMin; y < room.yMax; y++)
            {
                Vector2Int currentPos = new Vector2Int(x, y);

                if (!floorPositions.Contains(currentPos)) continue;

                foreach (var dir in directions)
                {
                    Vector2Int neighbor = currentPos + dir;

                    if (floorPositions.Contains(neighbor))
                    {
                        if (!room.Contains(new Vector3Int(neighbor.x, neighbor.y, 0)))
                        {
                            float dist = Vector2.Distance(neighbor, targetCenter);
                            
                            if (dist < minDistance)
                            {
                                minDistance = dist;
                                bestExit = neighbor; 
                            }
                        }
                    }
                }
            }
        }

        if (bestExit == Vector2Int.zero)
        {
            float fallbackDistance = float.MaxValue;
            foreach (var pos in floorPositions)
            {
                if (room.Contains(new Vector3Int(pos.x, pos.y, 0)))
                {
                    float d = Vector2.Distance(pos, targetCenter);
                    if (d < fallbackDistance)
                    {
                        fallbackDistance = d;
                        bestExit = pos;
                    }
                }
            }
        }

        return bestExit;
    }

    public Vector2Int GetCorridorEntrancePoint(BoundsInt currentRoom, List<BoundsInt> path)
    {
        int currentIndex = path.IndexOf(currentRoom);

        if (currentIndex <= 0) 
        {
            return DungeonHelper.GetRoomCenter(currentRoom); 
        }
        
        BoundsInt previousRoom = path[currentIndex - 1]; 
        
        return GetCorridorExitPoint(previousRoom, path);
    }

    private bool IsPointInAnyRoom(Vector2Int point, List<BoundsInt> rooms)
    {
        Vector3Int checkPoint = new Vector3Int(point.x, point.y, 0);

        foreach (var room in rooms)
        {
            if (checkPoint.x >= room.xMin && checkPoint.x < room.xMax &&
                checkPoint.y >= room.yMin && checkPoint.y < room.yMax)
            {
                return true;
            }
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        if (debugMSTEdges == null || debugMSTEdges.Count == 0)
        {
            return; 
        }

        if (debugAllEdges != null && debugAllEdges.Count > 0)
        {
            Gizmos.color = fullGraphColor; 
            foreach (var edge in debugAllEdges)
            {
                Vector3 start = new Vector3(edge.U.x + 0.5f, edge.U.y + 0.5f, -1);
                Vector3 end = new Vector3(edge.V.x + 0.5f, edge.V.y + 0.5f, -1);
                Gizmos.DrawLine(start, end);
            }
        }

        if (debugMSTEdges != null && debugMSTEdges.Count > 0)
        {
            Gizmos.color = mstColor;
            foreach (var edge in debugMSTEdges)
            {
                Vector3 start = new Vector3(edge.U.x + 0.5f, edge.U.y + 0.5f, -2); 
                Vector3 end = new Vector3(edge.V.x + 0.5f, edge.V.y + 0.5f, -2);
                
                Gizmos.DrawLine(start, end);
                Gizmos.DrawLine(start + Vector3.right * 0.1f, end + Vector3.right * 0.1f);
                Gizmos.DrawLine(start + Vector3.up * 0.1f, end + Vector3.up * 0.1f);
            }
        }

        if (debugFinalEdges != null && debugFinalEdges.Count > 0)
        {
            Gizmos.color = finalPathColor; 
            foreach (var edge in debugFinalEdges)
            {
                Vector3 start = new Vector3(edge.U.x + 0.5f, edge.U.y + 0.5f, -3); 
                Vector3 end = new Vector3(edge.V.x + 0.5f, edge.V.y + 0.5f, -3);
                
                Gizmos.DrawLine(start, end);
                
                Vector3 center = (start + end) / 2;
                Gizmos.DrawWireSphere(center, 0.2f);
            }
        }
    }
}