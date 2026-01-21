using UnityEngine;
using System.Collections.Generic;
using System;

public class CharacterStats : MonoBehaviour, IStatsProvider
{
    [SerializeField] private StatSheet initialStatSheet;

    private Dictionary<StatDefinition, Stat> _stats;
    public event Action OnStatsReinitialized;

    private void Awake()
    {
        _stats = new Dictionary<StatDefinition, Stat>();
        if (initialStatSheet != null)
        {
            ApplyStatSheet(initialStatSheet);
        }
    }

    public void ApplyStatSheet(StatSheet newStatSheet)
    {
        if (newStatSheet == null) return;
        
        initialStatSheet = newStatSheet;
        _stats.Clear();
        foreach (var entry in newStatSheet.Stats)
        {
            _stats[entry.Definition] = new Stat(entry.BaseValue);
        }

        OnStatsReinitialized?.Invoke();
    }

    private void Update()
    {
        foreach (var stat in _stats.Values)
        {
            stat.UpdateModifiers(Time.deltaTime);
        }
    }
    
    public Stat GetStat(StatDefinition definition)
    {
        _stats.TryGetValue(definition, out var stat);
        return stat;
    }

    public float GetFinalStatValue(StatDefinition definition)
    {
        var stat = GetStat(definition);
        if (stat == null) return 0;

        float finalValue = stat.FinalValue;

        if (definition.HasMaximum)
        {
            finalValue = Mathf.Min(finalValue, definition.MaxValue);
        }
        if (definition.IsInteger)
        {
            finalValue = Mathf.RoundToInt(finalValue);
        }
        return finalValue;
    }
    
    public void AddModifier(StatDefinition definition, StatModifier modifier)
    {
        GetStat(definition)?.AddModifier(modifier);
    }
    
    public void RemoveModifier(StatDefinition definition, StatModifier modifier)
    {
        GetStat(definition)?.RemoveModifier(modifier);
    }

    public void RemoveAllModifiersFromSource(StatDefinition definition, string source)
    {
        if (definition != null)
        {
            var stat = GetStat(definition);
            stat?.RemoveAllModifiersFromSource(source);
        }
        else 
        {
            foreach (var stat in _stats.Values)
            {
                stat.RemoveAllModifiersFromSource(source);
            }
        }
    }

}