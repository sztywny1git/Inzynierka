using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SimpleWaveFunctionCollapse
{

    public enum TileType { Empty, Chest, Table, Torch, Wall, Barrel, Vase, Pillar, Rock }

    public class Cell
    {
        public Vector2Int Position;
        public List<TileType> PossibleOptions;
        public bool Collapsed = false;

        public Cell(Vector2Int pos, List<TileType> initialOptions)
        {
            Position = pos;
            PossibleOptions = new List<TileType>(initialOptions);
        }
    }

    private Dictionary<TileType, List<TileType>> adjacencyRules;

    public SimpleWaveFunctionCollapse()
    {
        InitializeRules();
    }

    private void InitializeRules()
    {
        adjacencyRules = new Dictionary<TileType, List<TileType>>();

        var all = System.Enum.GetValues(typeof(TileType)).Cast<TileType>().ToList();
        
        adjacencyRules[TileType.Empty] = all;

        adjacencyRules[TileType.Chest] = new List<TileType> { TileType.Empty, TileType.Wall, TileType.Table };

        adjacencyRules[TileType.Torch] = new List<TileType> { TileType.Empty, TileType.Wall };

        adjacencyRules[TileType.Table] = new List<TileType> { TileType.Empty, TileType.Chest };

        adjacencyRules[TileType.Wall] = all; 
    }

    public Dictionary<Vector2Int, TileType> Run(BoundsInt roomBounds, HashSet<Vector2Int> floorPositions, int iterations = 1000)
    {
        List<Cell> grid = new List<Cell>();
        var allTypes = System.Enum.GetValues(typeof(TileType)).Cast<TileType>().Where(t => t != TileType.Wall).ToList();

        foreach (var pos in floorPositions)
        {
            if(roomBounds.Contains(new Vector3Int(pos.x, pos.y, 0)))
            {
                grid.Add(new Cell(pos, allTypes));
            }
        }

        for (int i = 0; i < iterations; i++)
        {

            Cell cellToCollapse = grid
                .Where(c => !c.Collapsed)
                .OrderBy(c => c.PossibleOptions.Count)
                .ThenBy(c => Random.value)          
                .FirstOrDefault();

            if (cellToCollapse == null) break;

            if (cellToCollapse.PossibleOptions.Count == 0)
            {
                cellToCollapse.PossibleOptions = new List<TileType> { TileType.Empty };
            }
            
            TileType selected = cellToCollapse.PossibleOptions[Random.Range(0, cellToCollapse.PossibleOptions.Count)];
            cellToCollapse.PossibleOptions = new List<TileType> { selected };
            cellToCollapse.Collapsed = true;

            Propagate(cellToCollapse, grid);
        }

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
                
                int originalCount = neighbor.PossibleOptions.Count;
                neighbor.PossibleOptions.RemoveAll(option => !allowedNeighbors.Contains(option));
            }
        }
    }
}