using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EnemyRegistry", menuName = "Core/Registries/Enemy Registry")]
public class EnemyRegistry : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        public EnemyType Type;
        public GameObject Prefab;
    }

    [SerializeField] private List<Entry> _entries;

    private Dictionary<EnemyType, GameObject> _lookup;

    private void InitializeLookup()
    {
        if (_lookup == null)
        {
            _lookup = new Dictionary<EnemyType, GameObject>();
            foreach (var entry in _entries)
            {
                if (entry.Prefab != null)
                {
                    _lookup.TryAdd(entry.Type, entry.Prefab);
                }
            }
        }
    }

    public GameObject GetPrefab(EnemyType type)
    {
        InitializeLookup();
        if (_lookup.TryGetValue(type, out var prefab))
        {
            return prefab;
        }

        Debug.LogError($"[EnemyRegistry] No prefab found for EnemyType: {type}");
        return null;
    }
}