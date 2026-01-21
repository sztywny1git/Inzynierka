using UnityEngine;
using System.Linq;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewBinarySwitchPuzzle", menuName = "Dungeon/Puzzle Template/Binary Switch")]
public class BinarySwitchPuzzleTemplate : PuzzleTemplate
{
    [Header("Konfiguracja")]
    [Tooltip("Ile bitów/przełączników ma zagadka (np. 4 daje zakres 0-15).")]
    public int bitCount = 8;
    public override PuzzleData GeneratePuzzleData(BoundsInt room)
    {
        PuzzleData data = new PuzzleData();
        
        // 1. Ustaw typ szablonu (do debugowania i spawnowania)
        data.PuzzleTemplateType = nameof(BinarySwitchPuzzleTemplate);
        
        // 2. Wylosuj liczbę docelową (TargetValue)
        // Dla 4 bitów: zakres 1 do 15 (unikamy 0, bo to często stan początkowy)
        int maxVal = (1 << bitCount) - 1; // np. 2^4 - 1 = 15
        data.TargetValue = Random.Range(1, maxVal + 1);
        
        Debug.Log($"[TEMPLATE] Generowanie zagadki. BitCount: {bitCount}, MaxVal: {maxVal}, WYLOSOWANO: {data.TargetValue}");
        
        // 3. Zainicjalizuj puste listy, aby uniknąć NullReference
        data.PressurePlatePositions = new System.Collections.Generic.List<Vector2Int>();
        data.CorrectSequence = new System.Collections.Generic.List<int>();

        // Pozycja Konsoli (PuzzleObjectPosition) zostanie nadpisana w PuzzleManager (na środek pokoju),
        // więc tutaj nie musimy jej precyzyjnie ustawiać.
        
        return data;
    }
}
