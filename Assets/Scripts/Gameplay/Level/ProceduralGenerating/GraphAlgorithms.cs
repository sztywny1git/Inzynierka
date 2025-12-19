using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GraphAlgorithms
{
    // Struktura reprezentująca połączenie między dwoma pokojami
    public struct Edge
    {
        public Vector2Int U; // Środek pokoju A
        public Vector2Int V; // Środek pokoju B
        public float Distance;

        public Edge(Vector2Int u, Vector2Int v)
        {
            U = u;
            V = v;
            Distance = Vector2.Distance(u, v);
        }
    }

    // Algorytm Kruskala: Zwraca listę krawędzi tworzących Minimalne Drzewo Rozpinające (MST)
    public static List<Edge> KruskalMST(List<Vector2Int> nodes, List<Edge> edges)
    {
        List<Edge> mst = new List<Edge>();
        
        // 1. Sortuj krawędzie od najkrótszej do najdłuższej
        edges.Sort((a, b) => a.Distance.CompareTo(b.Distance));

        // 2. Inicjalizacja Union-Find (zbiory rozłączne)
        UnionFind uf = new UnionFind(nodes);

        foreach (var edge in edges)
        {
            // Jeśli pokoje U i V są w różnych zbiorach (nie są jeszcze połączone)
            if (uf.Find(edge.U) != uf.Find(edge.V))
            {
                mst.Add(edge);      // Dodaj krawędź do MST
                uf.Union(edge.U, edge.V); // Połącz zbiory
            }
        }
        return mst;
    }

    // Klasa pomocnicza dla Kruskala
    private class UnionFind
    {
        private Dictionary<Vector2Int, Vector2Int> parent = new Dictionary<Vector2Int, Vector2Int>();

        public UnionFind(List<Vector2Int> nodes)
        {
            foreach (var node in nodes)
                parent[node] = node;
        }

        public Vector2Int Find(Vector2Int k)
        {
            if (parent[k] == k) return k;
            return parent[k] = Find(parent[k]); // Kompresja ścieżki
        }

        public void Union(Vector2Int a, Vector2Int b)
        {
            var rootA = Find(a);
            var rootB = Find(b);
            if (rootA != rootB) parent[rootA] = rootB;
        }
    }
}