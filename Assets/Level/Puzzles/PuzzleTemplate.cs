using UnityEngine;
using System.Collections.Generic;

// Bazowa klasa dla wszystkich typów zagadek
public abstract class PuzzleTemplate : ScriptableObject
{
    [Header("Wspólne Ustawienia")]
    public string puzzleName = "Nowa Zagadka";

    public RoomFirstDungeonGenerator DungeonGenerator { get; set; } 

    // Abstrakcyjna metoda, która będzie generować pozycje obiektów,
    // sekwencje, wagi itp. w oparciu o wymiary danego pokoju.
    public abstract PuzzleData GeneratePuzzleData(BoundsInt room);
}