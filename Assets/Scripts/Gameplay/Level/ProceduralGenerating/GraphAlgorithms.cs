using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GraphAlgorithms
{
    // Dodajemy IEquatable, aby HashSet wiedział, jak porównywać krawędzie
    public struct Edge : IEquatable<Edge>
    {
        public Vector2Int U;
        public Vector2Int V;
        public float Distance;

        public Edge(Vector2Int u, Vector2Int v)
        {
            // Zawsze zapisuj "mniejszy" wektor jako U, a "większy" jako V.
            // Dzięki temu krawędź A->B jest matematycznie tożsama z B->A.
            // Zapobiega to rysowaniu podwójnych korytarzy.
            if (u.x < v.x || (u.x == v.x && u.y < v.y))
            {
                U = u;
                V = v;
            }
            else
            {
                U = v;
                V = u;
            }
            Distance = Vector2.Distance(u, v);
        }

        // Metody wymagane przez IEquatable i HashSet
        public bool Equals(Edge other)
        {
            return U == other.U && V == other.V;
        }

        public override bool Equals(object obj)
        {
            return obj is Edge other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(U, V);
        }
    }

    public static List<Edge> KruskalMST(List<Vector2Int> nodes, List<Edge> edges)
    {
        List<Edge> mst = new List<Edge>();
        
        // Sortujemy krawędzie od najkrótszej (najważniejszy krok Kruskala)
        edges.Sort((a, b) => a.Distance.CompareTo(b.Distance));

        UnionFind uf = new UnionFind(nodes);

        foreach (var edge in edges)
        {
            // Jeśli wierzchołki są w różnych zbiorach, dodaj krawędź (nie tworzy cyklu)
            if (uf.Find(edge.U) != uf.Find(edge.V))
            {
                mst.Add(edge);
                uf.Union(edge.U, edge.V);
            }
        }
        return mst;
    }

    private class UnionFind
    {
        private Dictionary<Vector2Int, Vector2Int> parent = new Dictionary<Vector2Int, Vector2Int>();

        public UnionFind(List<Vector2Int> nodes)
        {
            foreach (var node in nodes)
                parent[node] = node; // Na początku każdy jest swoim rodzicem
        }

        public Vector2Int Find(Vector2Int k)
        {
            if (!parent.ContainsKey(k)) return k; // Zabezpieczenie
            if (parent[k] == k) return k;
            return parent[k] = Find(parent[k]); // Kompresja ścieżki (optymalizacja)
        }

        public void Union(Vector2Int a, Vector2Int b)
        {
            var rootA = Find(a);
            var rootB = Find(b);
            if (rootA != rootB) parent[rootA] = rootB;
        }
    }
}