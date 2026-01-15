using UnityEngine;
using System.Linq;
using System.Collections.Generic;

// Atrybut pozwala na łatwe tworzenie nowego szablonu z menu Assets/Create
[CreateAssetMenu(fileName = "NewSequencePuzzle", menuName = "Dungeon/Puzzle Template/Sequence")]
public class SequencePuzzleTemplate : PuzzleTemplate
{
    [Header("Ustawienia Sekwencji")]
    public int minPlates = 3;
    public int maxPlates = 5;
    
    // NADPISANIE abstrakcyjnej metody
    public override PuzzleData GeneratePuzzleData(BoundsInt room)
    {
        var puzzle = new PuzzleData();
        Vector2Int center = new Vector2Int(room.xMin + room.size.x / 2, room.yMin + room.size.y / 2);
        
        // Załóżmy, że obszar gry to 8x8 (od -4 do 4 od środka)
        int hx = Mathf.Min(room.size.x / 2, 4); 
        int hy = Mathf.Min(room.size.y / 2, 4);

        int numberOfPlates = Random.Range(minPlates, maxPlates + 1);
        
        HashSet<Vector2Int> usedPositions = new HashSet<Vector2Int>();
        
        // 1. Generowanie pozycji płytek
        for (int i = 0; i < numberOfPlates; i++)
        {
            Vector2Int pos;
            do
            {
                int x = Random.Range(-hx, hx + 1);
                int y = Random.Range(-hy, hy + 1);
                pos = center + new Vector2Int(x, y);
            } while (usedPositions.Contains(pos));

            usedPositions.Add(pos);
            puzzle.PressurePlatePositions.Add(pos);
        }

        // 2. Generowanie losowej sekwencji
        List<int> sequence = Enumerable.Range(0, numberOfPlates).ToList();
        // Użycie Random.Range z UnityEngine
        for (int i = numberOfPlates - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (sequence[i], sequence[j]) = (sequence[j], sequence[i]);
        }
        puzzle.CorrectSequence = sequence;

        // 3. Pozycja bramy (np. na górnej krawędzi)
        puzzle.GatePosition = center + new Vector2Int(0, hy + 1); 

        return puzzle;
    }
}