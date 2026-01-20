using UnityEngine;
using System;

/// <summary>
/// Scales enemy stats based on the current level.
/// Attach this to enemy prefabs or use EnemySpawner to apply scaling on spawn.
/// </summary>
public class EnemyScaler : MonoBehaviour
{
    [Header("Stat Definitions to Scale")]
    [SerializeField] private StatDefinition healthStatDef;
    [SerializeField] private StatDefinition damageStatDef;
    [SerializeField] private StatDefinition armorStatDef;
    
    [Header("Scaling Settings")]
    [Tooltip("Base multiplier applied per level (e.g., 0.1 = +10% per level)")]
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

    /// <summary>
    /// Applies scaling modifiers based on the given level.
    /// Called automatically by EnemySpawner after instantiation.
    /// </summary>
    /// <param name="level">The current game/dungeon level (1-based)</param>
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

        // Remove previous scaling if any
        if (_appliedLevel > 0)
        {
            RemoveScaling();
        }

        _appliedLevel = level;

        if (level <= 1)
        {
            // Level 1 = base stats, no scaling needed
            return;
        }

        // Calculate scaling multipliers (level 2 = first bonus, etc.)
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

    /// <summary>
    /// Removes all scaling modifiers from this enemy.
    /// </summary>
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

    /// <summary>
    /// Gets info about the current scaling applied.
    /// </summary>
    public int AppliedLevel => _appliedLevel;
}
