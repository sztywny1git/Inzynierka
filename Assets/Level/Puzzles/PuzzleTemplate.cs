using UnityEngine;
using System.Collections.Generic;
public abstract class PuzzleTemplate : ScriptableObject
{
    [Header("Wspólne Ustawienia")]
    public string puzzleName = "Nowa Zagadka";

    public RoomFirstDungeonGenerator DungeonGenerator { get; set; } 
    public abstract PuzzleData GeneratePuzzleData(BoundsInt room);
}