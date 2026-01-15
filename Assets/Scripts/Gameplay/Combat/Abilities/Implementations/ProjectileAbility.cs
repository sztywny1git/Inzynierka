using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Projectile")]
public class ProjectileAbility : DamageAbility
{
    private struct ProjectileData : IAbilityData
    {
        public CombatStats CombatStats;
        public int ProjectileCount;
        public int PierceCount;
    }

    [Header("Behavior")]
    [SerializeField] private ProjectileMovementConfig _movementConfig;

    [Header("Common Settings")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private float _baseSpeed = 20f;
    [SerializeField] private float _lifetime = 5f;
    [SerializeField] private float _spreadAngle = 15f;

    public override IAbilityData CreateData(IStatsProvider stats, StatSystemConfig config)
    {
        var combatStats = GetBaseCombatStats(stats, config);
        
        var data = new ProjectileData
        {
            CombatStats = combatStats,
            ProjectileCount = 1,
            PierceCount = 0
        };

        if (config.ProjectileCountStat != null)
        {
            int count = Mathf.FloorToInt(stats.GetFinalStatValue(config.ProjectileCountStat));
            data.ProjectileCount = Mathf.Max(1, count);
        }

        if (config.PierceCountStat != null)
        {
            data.PierceCount = Mathf.FloorToInt(stats.GetFinalStatValue(config.PierceCountStat));
        }

        return data;
    }

    public override void Execute(AbilityContext context, IAbilityData rawData)
    {
        if (_movementConfig == null || rawData is not ProjectileData data) return;

        Vector3 aimDir = (context.AimLocation - context.Origin.position).normalized;
        aimDir.z = 0;
        
        float startAngle = (data.ProjectileCount > 1) ? -_spreadAngle / 2f : 0f;
        float angleStep = (data.ProjectileCount > 1) ? _spreadAngle / (data.ProjectileCount - 1) : 0f;

        for (int i = 0; i < data.ProjectileCount; i++)
        {
            DamageData damagePayload = CalculateDamage(context, data.CombatStats);
            
            
            float currentAngle = startAngle + (angleStep * i);
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, aimDir) * Quaternion.Euler(0, 0, currentAngle);
            
            GameObject projectileObj = context.Spawner.Spawn(_projectilePrefab, context.Origin.position, rotation);
            
            IMovementStrategy strategy = _movementConfig.CreateStrategy(
                context.Origin.position,
                rotation,
                context.Origin.position + (rotation * Vector3.up * Vector3.Distance(context.Origin.position, context.AimLocation)),
                _baseSpeed
            );
            
            Projectile projectile = projectileObj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Initialize(
                    (obj) => context.Spawner.Despawn(obj),
                    context.Instigator,
                    strategy,
                    damagePayload,
                    data.PierceCount,
                    _lifetime
                );
            }
        }
    }
}