using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PropWFC
{
    public enum PropType 
    { 
        Empty, 
        Pillar, 
        Torch, 
        Crate, 
        Barrel, 
        Stone, 
        Vase, 
        Rug,   
        Fireplace   
    }

    public class Cell
    {
        public Vector2Int Position;
        public List<PropType> PossibleOptions;
        public bool Collapsed = false;
        public bool IsNextToWall = false;
        
        // Dynamiczne wagi dla tej komórki
        public Dictionary<PropType, int> LocalWeights; 

        // Śledzenie "pokolenia" dywanu. 
        // 0 = środek, 1 = pierwsza obwódka, 2 = druga obwódka itd.
        public int RugDistance = 0;

        public Cell(Vector2Int pos, bool nextToWall, Dictionary<PropType, int> globalWeights)
        {
            Position = pos;
            IsNextToWall = nextToWall;
            PossibleOptions = new List<PropType>((PropType[])System.Enum.GetValues(typeof(PropType)));
            
            // Kopia wag globalnych
            LocalWeights = new Dictionary<PropType, int>(globalWeights);
        }
    }

    // KONFIGURACJA ROZMIARU DYWANU
    // 1 = rozmiar 3x3 (środek + 1 pole w każdą stronę)
    // 2 = rozmiar 5x5 (środek + 2 pola)
    private const int MaxRugRadius = 1; 

    public Dictionary<Vector2Int, PropType> Run(
        BoundsInt room, 
        HashSet<Vector2Int> floor, 
        HashSet<Vector2Int> walls, 
        HashSet<Vector2Int> reservedPositions,
        Dictionary<PropType, int> roomWeights 
    )
    {
        List<Cell> grid = new List<Cell>();
        Dictionary<Vector2Int, Cell> gridLookup = new Dictionary<Vector2Int, Cell>();

        foreach (var pos in floor)
        {
            // Sprawdzanie granic 2D
            if (pos.x >= room.xMin && pos.x < room.xMax && 
                pos.y >= room.yMin && pos.y < room.yMax)
            {
                bool nextToWall = CheckIfNextToWall(pos, walls);
                
                // PRZEKAZUJEMY dynamiczne wagi do komórki
                var cell = new Cell(pos, nextToWall, roomWeights);
                
                if (reservedPositions.Contains(pos))
                {
                    cell.PossibleOptions.Clear();
                    cell.PossibleOptions.Add(PropType.Empty);
                    cell.Collapsed = true; 
                }
                else
                {
                    ApplyStaticConstraints(cell);
                }
                
                grid.Add(cell);
                gridLookup[pos] = cell;
            }
        }

        // --- FAZA STARTOWA DLA KLASTRÓW ---
        var possibleStarts = grid.Where(c => !c.Collapsed && !c.IsNextToWall && c.PossibleOptions.Contains(PropType.Rug)).ToList();
        
        // UWAGA: Sprawdzamy czy w ogóle Rug ma wagę > 0 w tym pokoju, zanim spróbujemy go zespawnować
        if (roomWeights.ContainsKey(PropType.Rug) && roomWeights[PropType.Rug] > 0 && possibleStarts.Count > 0)
        {
            if (Random.value < 0.7f)
            {
                Cell seed = possibleStarts[Random.Range(0, possibleStarts.Count)];
                seed.LocalWeights[PropType.Rug] += 1000;
            }
        }

        // --- GŁÓWNA PĘTLA ---
        int iterations = grid.Count(c => !c.Collapsed); 
        while (iterations > 0)
        {
             List<Cell> candidates = grid.Where(c => !c.Collapsed).ToList();
             if (candidates.Count == 0) break;

             candidates.Sort((a, b) => a.PossibleOptions.Count.CompareTo(b.PossibleOptions.Count));
             int minEntropy = candidates[0].PossibleOptions.Count;
            
             var minEntropyCells = candidates.Where(c => c.PossibleOptions.Count == minEntropy).ToList();
             Cell cellToCollapse = minEntropyCells[Random.Range(0, minEntropyCells.Count)];

             CollapseCell(cellToCollapse, gridLookup); 
             PropagateConstraints(cellToCollapse, gridLookup);

             iterations--;
        }

        return grid.ToDictionary(c => c.Position, c => c.PossibleOptions[0]);
    }

    private void ApplyStaticConstraints(Cell cell)
    {
        if (!cell.IsNextToWall)
        {
            cell.PossibleOptions.Remove(PropType.Torch);
            cell.PossibleOptions.Remove(PropType.Crate);
            cell.PossibleOptions.Remove(PropType.Barrel);
            cell.PossibleOptions.Remove(PropType.Stone);
            cell.PossibleOptions.Remove(PropType.Vase);
        }

        if (cell.IsNextToWall)
        {
            cell.PossibleOptions.Remove(PropType.Pillar);
            cell.PossibleOptions.Remove(PropType.Rug);
        }
    }

    private void CollapseCell(Cell cell, Dictionary<Vector2Int, Cell> gridLookup)
    {
        int totalWeight = 0;
        foreach (var option in cell.PossibleOptions) totalWeight += cell.LocalWeights[option];

        int randomValue = Random.Range(0, totalWeight);
        PropType selected = PropType.Empty;

        foreach (var option in cell.PossibleOptions)
        {
            randomValue -= cell.LocalWeights[option];
            if (randomValue < 0)
            {
                selected = option;
                break;
            }
        }

        cell.PossibleOptions.Clear();
        cell.PossibleOptions.Add(selected);
        cell.Collapsed = true;

        // === NOWOŚĆ: Obliczanie dystansu dywanu ===
        if (selected == PropType.Rug)
        {
            int minNeighborDist = 1000;
            bool foundParent = false;

            foreach (var dir in Direction2D.eightDirectionList)
            {
                Vector2Int nPos = cell.Position + dir;
                if (gridLookup.TryGetValue(nPos, out Cell neighbor) && neighbor.Collapsed)
                {
                    // Jeśli sąsiad jest dywanem, dziedziczymy jego dystans + 1
                    if (neighbor.PossibleOptions[0] == PropType.Rug)
                    {
                        if (neighbor.RugDistance < minNeighborDist)
                        {
                            minNeighborDist = neighbor.RugDistance;
                            foundParent = true;
                        }
                    }
                }
            }

            if (foundParent)
                cell.RugDistance = minNeighborDist + 1;
            else
                cell.RugDistance = 0; // To jest "ziarno" (środek)
        }
    }

    private void PropagateConstraints(Cell sourceCell, Dictionary<Vector2Int, Cell> gridLookup)
    {
        PropType type = sourceCell.PossibleOptions[0];

        // --- LOGIKA OGRANICZONEGO DYWANU ---
        if (type == PropType.Rug)
        {
            bool reachedLimit = sourceCell.RugDistance >= MaxRugRadius;

            foreach (var dir in Direction2D.eightDirectionList)
            {
                Vector2Int neighborPos = sourceCell.Position + dir;
                if (gridLookup.TryGetValue(neighborPos, out Cell neighbor) && !neighbor.Collapsed)
                {
                    if (reachedLimit)
                    {
                        // Osiągnęliśmy limit wielkości!
                        // Zamiast zachęcać, ZABRANIAMY sąsiadowi bycia dywanem.
                        // To tworzy ładną, twardą krawędź klastra.
                        neighbor.PossibleOptions.Remove(PropType.Rug);
                    }
                    else
                    {
                        // Nie osiągnęliśmy limitu - propagujemy "wirusa" dywanowego.
                        if (neighbor.PossibleOptions.Contains(PropType.Rug))
                        {
                            neighbor.LocalWeights[PropType.Rug] += 500; // Silna atrakcja
                        }
                    }
                    
                    // Standardowe: dywan nie lubi ognia i filarów
                    neighbor.PossibleOptions.Remove(PropType.Fireplace);
                    neighbor.PossibleOptions.Remove(PropType.Pillar);
                }
            }
        }
        // ------------------------------------

        // --- OGNISKO ---
        if (type == PropType.Fireplace)
        {
            foreach (var dir in Direction2D.eightDirectionList)
            {
                Vector2Int neighborPos = sourceCell.Position + dir;
                if (gridLookup.TryGetValue(neighborPos, out Cell neighbor) && !neighbor.Collapsed)
                {
                    // Ogień wypala dywany i skrzynie
                    neighbor.PossibleOptions.Remove(PropType.Crate);
                    neighbor.PossibleOptions.Remove(PropType.Barrel);
                    neighbor.PossibleOptions.Remove(PropType.Rug);
                }
            }
        }

        // --- FILAR ---
        if (type == PropType.Pillar)
        {
            foreach (var dir in Direction2D.eightDirectionList)
            {
                Vector2Int neighborPos = sourceCell.Position + dir;
                if (gridLookup.TryGetValue(neighborPos, out Cell neighbor) && !neighbor.Collapsed)
                {
                    neighbor.PossibleOptions.Clear();
                    neighbor.PossibleOptions.Add(PropType.Empty);
                }
            }
        }

        // --- SKRZYNIE/BECZKI ---
        if (type == PropType.Crate || type == PropType.Barrel)
        {
             foreach (var dir in Direction2D.eightDirectionList)
            {
                Vector2Int neighborPos = sourceCell.Position + dir;
                if (gridLookup.TryGetValue(neighborPos, out Cell neighbor) && !neighbor.Collapsed)
                {
                    neighbor.PossibleOptions.Remove(PropType.Fireplace);
                    neighbor.PossibleOptions.Remove(PropType.Pillar);
                }
            }           
        }

        // --- INNE (Blokada filarów) ---
        if (type != PropType.Empty && type != PropType.Rug) 
        {
             foreach (var dir in Direction2D.eightDirectionList)
            {
                Vector2Int neighborPos = sourceCell.Position + dir;
                if (gridLookup.TryGetValue(neighborPos, out Cell neighbor) && !neighbor.Collapsed)
                {
                    neighbor.PossibleOptions.Remove(PropType.Pillar);
                }
            }
        }
    }

    private bool CheckIfNextToWall(Vector2Int pos, HashSet<Vector2Int> walls)
    {
        foreach (var dir in Direction2D.cardinalDirectionList)
        {
            if (walls.Contains(pos + dir)) return true;
        }
        return false;
    }
}