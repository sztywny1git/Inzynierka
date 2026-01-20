using UnityEngine;
using System;

public class EnemyScaler : MonoBehaviour
{
    [Header("Stat Definitions to Scale")]
    [SerializeField] private StatDefinition healthStatDef;
    [SerializeField] private StatDefinition damageStatDef;
    [SerializeField] private StatDefinition armorStatDef;
    
    [Header("Scaling Settings")]
    [SerializeField] private float healthScalePerLevel = 0.15f;
    [SerializeField] private float damageScalePerLevel = 0.1f;
    [SerializeField] private float armorScalePerLevel = 0.05f;
    
    private IStatsProvider _statsProvider;
    private int _appliedLevel = 0;
    private const string SCALING_SOURCE = "LevelScaling";

    private void Awake()
    {
        _statsProvider = GetComponent<IStatsProvider>();
    }

    public void ApplyLevelScaling(int level)
    {
        if (_statsProvider == null)
        {
            _statsProvider = GetComponent<IStatsProvider>();
        }

        if (_statsProvider == null)
        {
            Debug.LogWarning($"[EnemyScaler] No IStatsProvider found on '{name}'. Cannot apply scaling.", this);
            return;
        }

        if (_appliedLevel > 0)
        {
            RemoveScaling();
        }

        _appliedLevel = level;

        if (level <= 1)
        {
            return;
        }

        int scalingLevels = level - 1;
        
        ApplyScalingStat(healthStatDef, healthScalePerLevel, scalingLevels);
        ApplyScalingStat(damageStatDef, damageScalePerLevel, scalingLevels);
        ApplyScalingStat(armorStatDef, armorScalePerLevel, scalingLevels);
    }

    private void ApplyScalingStat(StatDefinition statDef, float scalePerLevel, int levels)
    {
        if (statDef == null) return;

        float percentBonus = scalePerLevel * levels;
        
        var modifier = new StatModifier(
            percentBonus,
            ModifierType.PercentAdd,
            SCALING_SOURCE
        );

        _statsProvider.AddModifier(statDef, modifier);
    }

    public void RemoveScaling()
    {
        if (_statsProvider == null) return;

        if (healthStatDef != null)
            _statsProvider.RemoveAllModifiersFromSource(healthStatDef, SCALING_SOURCE);
        if (damageStatDef != null)
            _statsProvider.RemoveAllModifiersFromSource(damageStatDef, SCALING_SOURCE);
        if (armorStatDef != null)
            _statsProvider.RemoveAllModifiersFromSource(armorStatDef, SCALING_SOURCE);

        _appliedLevel = 0;
    }

    public int AppliedLevel => _appliedLevel;
}
