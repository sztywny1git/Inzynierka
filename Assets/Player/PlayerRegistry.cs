using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "PlayerRegistry", menuName = "Core/Registries/Player Registry")]
public class PlayerRegistry : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        public CharacterDefinition Definition;
        public GameObject Prefab;
    }

    [SerializeField] private List<Entry> _entries;

    private Dictionary<CharacterDefinition, GameObject> _lookup;

    private void InitializeLookup()
    {
        if (_lookup == null)
        {
            _lookup = new Dictionary<CharacterDefinition, GameObject>();
            foreach (var entry in _entries)
            {
                if (entry.Definition != null && entry.Prefab != null)
                {
                    _lookup.TryAdd(entry.Definition, entry.Prefab);
                }
            }
        }
    }

    public GameObject GetPrefab(CharacterDefinition definition)
    {
        InitializeLookup();
        if (_lookup.TryGetValue(definition, out var prefab))
        {
            return prefab;
        }
        
        Debug.LogError($"[PlayerRegistry] No prefab found for definition: {definition.name}");
        return null;
    }
}