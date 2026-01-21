using UnityEngine;

public abstract class DamageAbility : Ability
{
    [SerializeField] protected float _damageMultiplier = 1.0f;
    [SerializeField] protected int _attackCount = 1;

    protected DamageData CalculateDamage(AbilityContext context, AbilitySnapshot snapshot)
    {
        float baseDmg = snapshot.BaseDamage * _damageMultiplier;
        
        bool isCrit = Random.value <= snapshot.CritChance;
        float finalDamage = isCrit ? baseDmg * snapshot.CritMultiplier : baseDmg;

        return new DamageData(finalDamage, isCrit, context.Instigator, context.Origin.position);
    }
}