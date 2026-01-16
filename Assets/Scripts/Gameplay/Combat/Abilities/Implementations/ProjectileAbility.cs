using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Projectile")]
public class ProjectileAbility : DamageAbility
{
    [Header("Behavior")]
    [SerializeField] private ProjectileMovementConfig _movementConfig;

    [Header("Projectile Settings")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private float _baseSpeed = 20f;
    [SerializeField] private float _lifetime = 5f;
    [SerializeField] private float _spreadAngle = 15f;
    [SerializeField] private int _pierceCount = 0; 

    public override void Execute(AbilityContext context, AbilitySnapshot snapshot)
    {
        if (_movementConfig == null || _projectilePrefab == null) return;

        Vector3 aimDir = (context.AimLocation - context.Origin.position).normalized;
        aimDir.z = 0;
        
        int count = Mathf.Max(1, _attackCount);
        
        float startAngle = (count > 1) ? -_spreadAngle / 2f : 0f;
        float angleStep = (count > 1) ? _spreadAngle / (count - 1) : 0f;

        for (int i = 0; i < count; i++)
        {
            DamageData damagePayload = CalculateDamage(context, snapshot);
            
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
                    _pierceCount,
                    _lifetime
                );
            }
        }
    }
}