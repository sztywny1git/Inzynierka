using UnityEngine;
using System.Linq;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewBinarySwitchPuzzle", menuName = "Dungeon/Puzzle Template/Binary Switch")]
public class BinarySwitchPuzzleTemplate : PuzzleTemplate
{
    [Header("Konfiguracja")]
    public int bitCount = 8;
    public override PuzzleData GeneratePuzzleData(BoundsInt room)
    {
        PuzzleData data = new PuzzleData();
        
        data.PuzzleTemplateType = nameof(BinarySwitchPuzzleTemplate);
        
        int maxVal = (1 << bitCount) - 1;
        data.TargetValue = Random.Range(1, maxVal + 1);
        
        //Debug.Log($"[TEMPLATE] Generowanie zagadki. BitCount: {bitCount}, MaxVal: {maxVal}, WYLOSOWANO: {data.TargetValue}");
        
        data.PressurePlatePositions = new System.Collections.Generic.List<Vector2Int>();
        data.CorrectSequence = new System.Collections.Generic.List<int>();
        
        return data;
    }
}
