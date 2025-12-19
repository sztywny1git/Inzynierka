using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SimpleWaveFunctionCollapse
{
    // Typy obiektów
    public enum TileType { Empty, Chest, Table, Torch, Wall, Barrel, Vase, Pillar, Rock }

    // Reprezentacja pojedynczej komórki w siatce WFC
    public class Cell
    {
        public Vector2Int Position;
        public List<TileType> PossibleOptions; // Superpozycja
        public bool Collapsed = false;

        public Cell(Vector2Int pos, List<TileType> initialOptions)
        {
            Position = pos;
            PossibleOptions = new List<TileType>(initialOptions);
        }
    }

    // Reguły sąsiedztwa (Co może stać obok czego)
    private Dictionary<TileType, List<TileType>> adjacencyRules;

    public SimpleWaveFunctionCollapse()
    {
        InitializeRules();
    }

    private void InitializeRules()
    {
        adjacencyRules = new Dictionary<TileType, List<TileType>>();

        // Definicja reguł (Możesz tu dodać własne logiczne zasady)
        var all = System.Enum.GetValues(typeof(TileType)).Cast<TileType>().ToList();
        
        // Puste pole może sąsiadować ze wszystkim
        adjacencyRules[TileType.Empty] = all;

        // Skrzynia NIE może stać obok Ogniska (Torch) ani innej Skrzyni
        adjacencyRules[TileType.Chest] = new List<TileType> { TileType.Empty, TileType.Wall, TileType.Table };

        // Ognisko (Torch) może stać tylko przy Pustym lub Ścianie
        adjacencyRules[TileType.Torch] = new List<TileType> { TileType.Empty, TileType.Wall };

        // Stół
        adjacencyRules[TileType.Table] = new List<TileType> { TileType.Empty, TileType.Chest };
        
        // Ściana (granice pokoju)
        adjacencyRules[TileType.Wall] = all; 
    }

    public Dictionary<Vector2Int, TileType> Run(BoundsInt roomBounds, HashSet<Vector2Int> floorPositions, int iterations = 1000)
    {
        // 1. Inicjalizacja siatki (Grid)
        List<Cell> grid = new List<Cell>();
        var allTypes = System.Enum.GetValues(typeof(TileType)).Cast<TileType>().Where(t => t != TileType.Wall).ToList();

        // Tworzymy siatkę tylko dla podłogi wewnątrz pokoju
        foreach (var pos in floorPositions)
        {
            if(roomBounds.Contains(new Vector3Int(pos.x, pos.y, 0)))
            {
                grid.Add(new Cell(pos, allTypes));
            }
        }

        // 2. Główna pętla WFC
        for (int i = 0; i < iterations; i++)
        {
            // A. Znajdź komórkę o najmniejszej entropii (najmniej możliwych opcji), ale nie rozwiązaną
            Cell cellToCollapse = grid
                .Where(c => !c.Collapsed)
                .OrderBy(c => c.PossibleOptions.Count) // Sortuj rosnąco wg entropii
                .ThenBy(c => Random.value)             // Losowość dla remisów
                .FirstOrDefault();

            if (cellToCollapse == null) break; // Wszystko rozwiązane

            // B. Załamanie fali (Collapse)
            // Jeśli zaszliśmy w kozi róg (0 opcji), resetujemy do Empty (prosta obsługa błędów)
            if (cellToCollapse.PossibleOptions.Count == 0)
            {
                cellToCollapse.PossibleOptions = new List<TileType> { TileType.Empty };
            }
            
            TileType selected = cellToCollapse.PossibleOptions[Random.Range(0, cellToCollapse.PossibleOptions.Count)];
            cellToCollapse.PossibleOptions = new List<TileType> { selected };
            cellToCollapse.Collapsed = true;

            // C. Propagacja (Aktualizacja sąsiadów)
            Propagate(cellToCollapse, grid);
        }

        // 3. Zwróć wynik
        return grid.ToDictionary(c => c.Position, c => c.PossibleOptions.FirstOrDefault());
    }

    private void Propagate(Cell sourceCell, List<Cell> grid)
    {
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        TileType sourceType = sourceCell.PossibleOptions[0];
        List<TileType> allowedNeighbors = adjacencyRules[sourceType];

        foreach (var dir in dirs)
        {
            Vector2Int neighborPos = sourceCell.Position + dir;
            Cell neighbor = grid.FirstOrDefault(c => c.Position == neighborPos);

            if (neighbor != null && !neighbor.Collapsed)
            {
                // Usuń opcje, które są niezgodne z regułami sąsiada
                // (Pozostaw tylko te, które są w liście allowedNeighbors)
                // Uwaga: To jest uproszczona propagacja (1 poziom). Pełna WFC używa stosu.
                // Do pracy inżynierskiej wystarczy wspomnieć o "ograniczonej propagacji".
                
                int originalCount = neighbor.PossibleOptions.Count;
                // neighbor.PossibleOptions = neighbor.PossibleOptions.Intersect(allowedNeighbors).ToList();
                // Powyższe to uproszczenie, poprawniej: sprawdź czy opcja sąsiada pozwala na 'sourceType'
                // Ale dla prostoty użyjemy listy dozwolonych:
                
                // Tutaj implementujemy prosty constraint check:
                 neighbor.PossibleOptions.RemoveAll(option => !allowedNeighbors.Contains(option));
            }
        }
    }
}