using UnityEngine;

public abstract class DamageAbility : Ability
{
    [SerializeField] protected float _damageMultiplier = 1.0f;

    protected CombatStats GetBaseCombatStats(IStatsProvider stats, StatSystemConfig config)
    {
        return new CombatStats
        {
            BaseDamage = stats.GetFinalStatValue(config.DamageStat) * _damageMultiplier,
            CritChance = stats.GetFinalStatValue(config.CritChanceStat),
            CritMultiplier = stats.GetFinalStatValue(config.CritMultiplierStat)
        };
    }

    protected DamageData CalculateDamage(AbilityContext context, CombatStats stats)
    {
        bool isCrit = UnityEngine.Random.value <= stats.CritChance;
        float finalDamage = isCrit ? stats.BaseDamage * stats.CritMultiplier : stats.BaseDamage;

        return new DamageData(finalDamage, isCrit, context.Instigator, context.Origin.position);
    }
}