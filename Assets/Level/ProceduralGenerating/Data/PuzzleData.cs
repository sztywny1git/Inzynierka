using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PuzzleData
{
    public List<Vector2Int> PressurePlatePositions = new List<Vector2Int>();
    
    public List<int> CorrectSequence = new List<int>();

    public Vector2Int GatePosition;

    public int CurrentProgressIndex = 0;

    public bool IsKeyPuzzle;
    
    public Vector2Int BlockedCorridorPosition; 

    public int TargetValue;

    public string PuzzleTemplateType ;
    public Vector2Int PuzzleObjectPosition;
}