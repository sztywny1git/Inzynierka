using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PuzzleManager : MonoBehaviour
{
    [Header("Dostępne Szablony Zagadek")]
    [SerializeField]
    private List<PuzzleTemplate> availablePuzzles;

    private Dictionary<Vector2Int, PuzzleData> activePuzzles = new Dictionary<Vector2Int, PuzzleData>();

    [SerializeField] private RoomFirstDungeonGenerator dungeonGenerator;

    [System.Serializable] 
    public struct PuzzleSaveEntry
    {
        public Vector2Int RoomCenter;
        public PuzzleData Data;
    }

    [SerializeField, HideInInspector] 
    private List<PuzzleSaveEntry> savedPuzzles = new List<PuzzleSaveEntry>();
    public HashSet<Vector2Int> ReservedPuzzlePositions = new HashSet<Vector2Int>(); 

    [Header("Puzzle Feedback")]
    [SerializeField] private AudioClip correctStepSFX;
    [SerializeField] private AudioClip incorrectStepSFX;
    [SerializeField] private GameObject successVFXPrefab;
    
    [Header("Nagroda")]
    [SerializeField] private GameObject shopKeeperPrefab;

    private AudioSource audioSource;

    private void Awake()
    {
        if (activePuzzles.Count == 0 && savedPuzzles.Count > 0)
        {
            foreach (var entry in savedPuzzles)
            {
                if (!activePuzzles.ContainsKey(entry.RoomCenter))
                {
                    activePuzzles.Add(entry.RoomCenter, entry.Data);
                }
            }
        }
        
        audioSource = GetComponent<AudioSource>(); 
    }

    public RoomFirstDungeonGenerator DungeonGenerator 
    {
        get
        {
            if (dungeonGenerator == null)
            {
                dungeonGenerator = FindFirstObjectByType<RoomFirstDungeonGenerator>();
            }
            return dungeonGenerator;
        }
        set
        {
            dungeonGenerator = value;
        }
    }

    public void PreparePuzzles(List<(BoundsInt room, bool isKey)> puzzleRoomsWithContext)
    {
        activePuzzles.Clear();
        savedPuzzles.Clear();
        
        if (availablePuzzles == null || availablePuzzles.Count == 0)
        {
            return;
        }

        List<PuzzleTemplate> templatesToUse = new List<PuzzleTemplate>();
        int numberOfRooms = puzzleRoomsWithContext.Count;

        for (int i = 0; i < numberOfRooms; i++)
        {
            templatesToUse.Add(availablePuzzles[i % availablePuzzles.Count]);
        }

        var shuffledPuzzles = availablePuzzles.OrderBy(x => Random.value).ToList();
        
        for (int i = 0; i < numberOfRooms; i++)
        {
            BoundsInt room = puzzleRoomsWithContext[i].room;
            bool isKey = puzzleRoomsWithContext[i].isKey;           
            Vector2Int roomCenter = GetRoomCenter(room);

            PuzzleTemplate template = shuffledPuzzles[i % shuffledPuzzles.Count];
            PuzzleData data = template.GeneratePuzzleData(room);

            data.PuzzleObjectPosition = roomCenter;
            data.IsKeyPuzzle = puzzleRoomsWithContext[i].isKey;

            if (!activePuzzles.ContainsKey(roomCenter))
            {
                activePuzzles.Add(roomCenter, data);
                savedPuzzles.Add(new PuzzleSaveEntry 
                { 
                    RoomCenter = roomCenter, 
                    Data = data 
                });
            }
            
            if (DungeonGenerator != null)
            {
                if (data.PressurePlatePositions != null && data.PressurePlatePositions.Count > 0)
                {
                    data.PressurePlatePositions = FilterToFloorOnly(data.PressurePlatePositions);
                    data.PressurePlatePositions = data.PressurePlatePositions.Distinct().ToList();

                    int validPlateCount = data.PressurePlatePositions.Count;

                    if (validPlateCount > 0)
                    {
                        int sequenceLength = 5; 
                        
                        data.CorrectSequence.Clear();

                        for (int k = 0; k < sequenceLength; k++)
                        {
                            int safeIndex = Random.Range(0, validPlateCount);
                            data.CorrectSequence.Add(safeIndex);
                        }
                    }
                    else
                    {
                        continue; 
                    }

                    if (data.PressurePlatePositions.Contains(data.PuzzleObjectPosition))
                    {
                        Vector2Int safePos = FindSafePositionForReward(
                            data.PuzzleObjectPosition, 
                            data.PressurePlatePositions, 
                            DungeonGenerator.floorPositions, 
                            room
                        );

                        if (safePos != Vector2Int.zero)
                        {
                            data.PuzzleObjectPosition = safePos;
                        }
                        else
                        {
                            data.PressurePlatePositions.Remove(data.PuzzleObjectPosition);
                        }
                    }
                }
                
                if (!DungeonGenerator.floorPositions.Contains(data.PuzzleObjectPosition))
                {
                    Vector2Int fallbackPos = FindNearestFloorTile(data.PuzzleObjectPosition, DungeonGenerator.floorPositions, room);
                    
                    if (fallbackPos != Vector2Int.zero)
                    {
                        data.PuzzleObjectPosition = fallbackPos;
                    }
                }
                
                if (isKey)
                {
                    Vector2Int exitPoint = DungeonGenerator.GetCorridorExitPoint(room, DungeonGenerator.MainPathRooms); 
                    
                    if (exitPoint != Vector2Int.zero)
                    {
                        data.GatePosition = exitPoint;
                    }
                }
            }
            
            data.IsKeyPuzzle = isKey;       
            if (!activePuzzles.ContainsKey(roomCenter))
            {
                activePuzzles.Add(roomCenter, data);
            }
        }
    }
    
    public void SpawnPuzzleObjects(BoundsInt roomLimits, Spawner spawner)
    {
        if (spawner == null)
        {
            return;
        }
        
        foreach (var entry in activePuzzles)
        {
            Vector2Int roomCenter = entry.Key; 
            PuzzleData puzzleValue = entry.Value;

            if (DungeonGenerator == null) 
            {
                DungeonGenerator = FindFirstObjectByType<RoomFirstDungeonGenerator>();
            }

            BoundsInt room = DungeonGenerator.FindRoomContainingPoint(DungeonGenerator.allRooms, roomCenter);
            if (activePuzzles.TryGetValue(roomCenter, out PuzzleData puzzle))
            {
                if (puzzle.PuzzleTemplateType == nameof(BinarySwitchPuzzleTemplate)) 
                {
                    spawner.SpawnBinaryConsole(puzzle.PuzzleObjectPosition, puzzle);
                }
                else
                {
                    foreach (var pos in puzzle.PressurePlatePositions)
                    {
                        spawner.SpawnPressurePlate(pos, DungeonGenerator); 
                    }
                }
                
                bool isSolved = false;

                if (puzzle.PuzzleTemplateType == nameof(BinarySwitchPuzzleTemplate))
                {
                    isSolved = (puzzle.CurrentProgressIndex == int.MaxValue);
                }
                else
                {
                    if (puzzle.CorrectSequence != null && puzzle.CorrectSequence.Count > 0)
                    {
                        isSolved = (puzzle.CurrentProgressIndex >= puzzle.CorrectSequence.Count);
                    }
                    else
                    {
                        isSolved = false;
                    }
                }

                if (puzzle.IsKeyPuzzle)
                {
                    if (puzzle.GatePosition != Vector2Int.zero)
                    {
                        spawner.SpawnGate(puzzle.GatePosition, DungeonGenerator, isSolved); 
                    }
                }
                else
                {
                    Debug.Log($"[Puzzle] Else branch entered. isSolved={isSolved}, shopKeeperPrefab={(shopKeeperPrefab != null)}");

                    if (isSolved && shopKeeperPrefab != null)
                    {
                        Vector3 spawnPos = new Vector3(
                            puzzle.PuzzleObjectPosition.x + 0.5f,
                            puzzle.PuzzleObjectPosition.y + 0.5f,
                            0
                        );

                        Debug.Log($"[Puzzle] Spawning ShopKeeper at position {spawnPos}");

                        Instantiate(shopKeeperPrefab, spawnPos, Quaternion.identity);
                    }
                    else
                    {
                        Debug.LogWarning("[Puzzle] ShopKeeper NOT spawned – puzzle not solved or prefab is null");
                    }
                }

            }
        }
    }

    public bool CheckPressurePlate(Vector2Int position)
    {
        RoomFirstDungeonGenerator generator = DungeonGenerator;

        if (generator == null || generator.allRooms == null) 
        { 
            return false;
        }
        
        BoundsInt currentRoomBounds = generator.FindRoomContainingPoint(DungeonGenerator.allRooms, position);

        Vector2Int roomCenterKey = GetRoomCenter(currentRoomBounds);

        if (!activePuzzles.ContainsKey(roomCenterKey))
        {
            return false;
        }

        PuzzleData puzzle = activePuzzles[roomCenterKey];
        
        int pressedPlateIndex = puzzle.PressurePlatePositions.IndexOf(position);

        if (pressedPlateIndex == -1)
        {
            return false;
        }
        
        if (puzzle.CurrentProgressIndex >= puzzle.CorrectSequence.Count)
        {
            return true;
        }

        int requiredPlateIndex = puzzle.CorrectSequence[puzzle.CurrentProgressIndex];

        if (pressedPlateIndex == requiredPlateIndex)
        {
            puzzle.CurrentProgressIndex++;

            if (audioSource != null && correctStepSFX != null)
            {
                audioSource.PlayOneShot(correctStepSFX);
            }

            if (puzzle.CurrentProgressIndex >= puzzle.CorrectSequence.Count)
            {
                if (puzzle.IsKeyPuzzle)
                {
                    if (DungeonGenerator.spawner != null)
                    {
                        DungeonGenerator.spawner.OpenGate(puzzle.GatePosition); 
                    }
                }
                else
                {
                    if (shopKeeperPrefab != null)
                    {
                        Vector3 spawnPos = new Vector3(puzzle.PuzzleObjectPosition.x + 0.5f, puzzle.PuzzleObjectPosition.y + 0.5f, 0);
                        Instantiate(shopKeeperPrefab, spawnPos, Quaternion.identity);
                    }
                }
                return true;
            }
        }
        else
        {
            puzzle.CurrentProgressIndex = 0;

            if (audioSource != null && incorrectStepSFX != null)
            {
                audioSource.PlayOneShot(incorrectStepSFX);
            }
        }

        activePuzzles[roomCenterKey] = puzzle;
        return false;
    }

    public void SolveBinaryPuzzle(Vector2Int consolePosition)
    {
        if (activePuzzles.ContainsKey(consolePosition))
        {
            PuzzleData puzzle = activePuzzles[consolePosition];
            
            puzzle.CurrentProgressIndex = int.MaxValue; 
            
            if (puzzle.IsKeyPuzzle)
            {
                if (DungeonGenerator.spawner != null)
                {
                    DungeonGenerator.spawner.OpenGate(puzzle.GatePosition);
                }
            }
            else
            {
                if (shopKeeperPrefab != null)
                {
                    Vector3 spawnPos = new Vector3(puzzle.PuzzleObjectPosition.x + 0.5f, puzzle.PuzzleObjectPosition.y + 0.5f, 0);
                    Instantiate(shopKeeperPrefab, spawnPos, Quaternion.identity);
                }
            }
            
            activePuzzles[consolePosition] = puzzle;
        }
    }

    private List<Vector2Int> FilterWallPositions(List<Vector2Int> positions)
    {
        HashSet<Vector2Int> wallPositions = DungeonGenerator.WallPositions;
        
        return positions.Where(pos => !wallPositions.Contains(pos)).ToList();
    }

    private List<Vector2Int> FilterToFloorOnly(List<Vector2Int> positions)
    {
        HashSet<Vector2Int> floorPositions = DungeonGenerator.floorPositions;
        
        return positions
            .Where(pos => floorPositions.Contains(pos))
            .ToList();
    }

    private Vector2Int FindNearestFloorTile(Vector2Int startPos, HashSet<Vector2Int> floorPositions, BoundsInt room)
    {
        int maxRadius = 5; 
        
        for (int r = 0; r <= maxRadius; r++)
        {
            for (int x = -r; x <= r; x++)
            {
                for (int y = -r; y <= r; y++)
                {
                    Vector2Int checkPos = startPos + new Vector2Int(x, y);
                    
                    if (floorPositions.Contains(checkPos)) 
                    {
                        if (room.Contains(new Vector3Int(checkPos.x, checkPos.y, 0)))
                        {
                            return checkPos; 
                        }
                    }
                }
            }
        }
        
        for (int x = room.xMin + 1; x < room.xMax - 1; x++)
        {
            for (int y = room.yMin + 1; y < room.yMax - 1; y++)
            {
                Vector2Int checkPos = new Vector2Int(x, y);
                if (floorPositions.Contains(checkPos))
                {
                    return checkPos; 
                }
            }
        }

        return Vector2Int.zero; 
    }

    private Vector2Int FindSafePositionForReward(Vector2Int startPos, List<Vector2Int> forbiddenPositions, HashSet<Vector2Int> floorPositions, BoundsInt room)
    {
        int maxRadius = 3; 

        for (int r = 1; r <= maxRadius; r++) 
        {
            for (int x = -r; x <= r; x++)
            {
                for (int y = -r; y <= r; y++)
                {
                    if (Mathf.Abs(x) != r && Mathf.Abs(y) != r) continue;

                    Vector2Int checkPos = startPos + new Vector2Int(x, y);

                    if (floorPositions.Contains(checkPos))
                    {
                        if (room.Contains(new Vector3Int(checkPos.x, checkPos.y, 0)))
                        {
                            if (!forbiddenPositions.Contains(checkPos))
                            {
                                return checkPos; 
                            }
                        }
                    }
                }
            }
        }
        return Vector2Int.zero; 
    }
    public bool TryGetPuzzleDataByRoom(Vector2Int roomKey, out PuzzleData data)
    {
        if (activePuzzles.ContainsKey(roomKey))
        {
            data = activePuzzles[roomKey];
            return true;
        }
        data = null;
        return false;
    }

    private Vector2Int GetRoomCenter(BoundsInt room)
    {
        int x = room.xMin + (room.size.x / 2);
        int y = room.yMin + (room.size.y / 2);
        
        return new Vector2Int(x, y);
    }
}