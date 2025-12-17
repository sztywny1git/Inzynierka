using UnityEngine;
using VContainer;

[CreateAssetMenu(fileName = "ProjectileAbility", menuName = "Abilities/Implementations/Projectile")]
public class ProjectileAbility : Ability
{
    [Header("Data Dependencies")]
    [SerializeField] private StatDefinition damageStat;
    [SerializeField] private StatDefinition attackSpeedStat;
    
    [Header("Projectile Properties")]
    [SerializeField] private string projectilePath;
    [SerializeField] private float baseDamage;
    [SerializeField] private float baseSpeed;
    [SerializeField] private int pierceCount;

    [Header("Trajectory Properties")]
    [SerializeField] private float fixedRange = 10f;
    [SerializeField] private float maxHeight = 2f;
    [SerializeField] private AnimationCurve trajectoryCurve;

    public override void Execute(AbilityContext context)
    {
        if (damageStat == null || attackSpeedStat == null)
        {
            Debug.LogError($"Ability {this.name} is missing StatDefinition dependencies!");
            return;
        }

        var projectileFactory = context.DIContainer.Resolve<IProjectileFactory>();

        float damageMultiplier = context.Stats.GetFinalStatValue(damageStat);
        float finalDamage = baseDamage * damageMultiplier;

        float speedMultiplier = context.Stats.GetFinalStatValue(attackSpeedStat);
        float finalSpeed = baseSpeed * speedMultiplier;
        
        Vector3 targetPosition = context.Origin.position + (Vector3)context.Direction.normalized * fixedRange;
        float journeyDuration = (finalSpeed > 0) ? fixedRange / finalSpeed : 0;
        
        var movementStrategy = new BaseMovementStrategy(
            target: targetPosition,
            duration: journeyDuration,
            maxHeight: maxHeight,
            curve: trajectoryCurve
        );
        
        projectileFactory.Spawn(
            owner: context.Owner,
            prefabPath: projectilePath,
            position: context.Origin.position,
            movementStrategy: movementStrategy,
            damage: finalDamage,
            pierceCount: pierceCount
        );
    }
}