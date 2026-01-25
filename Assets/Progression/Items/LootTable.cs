using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "New Loot Table", menuName = "Loot/Loot Table")]
public class LootTableSO : ScriptableObject
{
    [Header("Global Settings")]
    [Range(0f, 1f)] public float dropChance = 0.5f;

    [Serializable]
    public class LootPool
    {
        public string poolName;
        [Tooltip("Szansa na wylosowanie tej kategorii (względem sumy wszystkich wag)")]
        public float weight;
        public List<ItemSO> items;
    }

    [Header("Loot Pools")]
    public List<LootPool> pools = new List<LootPool>();

    public ItemSO GetRandomItem()
    {
        float totalWeight = 0;
        foreach (var pool in pools)
        {
            if (pool.items.Count > 0)
                totalWeight += pool.weight;
        }

        if (totalWeight <= 0) return null;

        float randomValue = UnityEngine.Random.Range(0, totalWeight);
        float currentWeight = 0;

        foreach (var pool in pools)
        {
            if (pool.items.Count == 0) continue;

            currentWeight += pool.weight;
            if (randomValue < currentWeight)
            {
                return pool.items[UnityEngine.Random.Range(0, pool.items.Count)];
            }
        }

        return null;
    }
}