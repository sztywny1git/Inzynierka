using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PuzzleData
{
    // Lista pozycji płytek naciskowych w pokoju
    public List<Vector2Int> PressurePlatePositions = new List<Vector2Int>();
    
    // Poprawna sekwencja, której musi przestrzegać gracz (indeksy w PressurePlatePositions)
    public List<int> CorrectSequence = new List<int>();

    // Zmieniamy GatePosition na pozycję drzwi/bramy (jeśli zagadka jest kluczowa)
    public Vector2Int GatePosition;

    // Bieżący stan postępu gracza w sekwencji
    public int CurrentProgressIndex = 0;

    public bool IsKeyPuzzle; // Czy ta zagadka blokuje główną ścieżkę?
    
    public Vector2Int BlockedCorridorPosition; // Zachowujemy tę nazwę do oznaczania wyjścia z korytarza (jeśli zagadka jest kluczowa)

    public int TargetValue; // Liczba docelowa (0-255) dla gracza

    public string PuzzleTemplateType ;// Typ szablonu (np. "BinarySwitchPuzzleTemplate")
    public Vector2Int PuzzleObjectPosition;
}