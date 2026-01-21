using UnityEngine;
using Cysharp.Threading.Tasks;

[CreateAssetMenu(menuName = "Abilities/Projectile")]
public class ProjectileAbility : DamageAbility
{
    [Header("Behavior")]
    [SerializeField] private ProjectileMovementConfig _movementConfig;

    [Header("Projectile Settings")]
    [SerializeField] private Projectile _projectilePrefab;
    [SerializeField] private float _baseSpeed = 20f;
    [SerializeField] private float _lifetime = 5f;
    [SerializeField] private float _spreadAngle = 15f;
    [SerializeField] private int _pierceCount = 0;

    public override UniTask Execute(AbilityContext context, AbilitySnapshot snapshot)
    {
        if (_movementConfig == null || _projectilePrefab == null) return UniTask.CompletedTask;

        Vector3 aimDiff = context.AimLocation - context.Origin.position;
        aimDiff.z = 0;
        Vector3 aimDir = aimDiff.normalized;

        if (aimDir == Vector3.zero) aimDir = Vector3.right;

        int count = Mathf.Max(1, _attackCount);
        
        float startAngle = (count > 1) ? -_spreadAngle / 2f : 0f;
        float angleStep = (count > 1) ? _spreadAngle / (count - 1) : 0f;

        for (int i = 0; i < count; i++)
        {
            DamageData damagePayload = CalculateDamage(context, snapshot);

            float currentAngleOffset = startAngle + (angleStep * i);
            Vector3 currentDir = Quaternion.Euler(0, 0, currentAngleOffset) * aimDir;

            Projectile projectile = context.Spawner.Spawn(_projectilePrefab, context.Origin.position, Quaternion.identity);

            if (projectile == null) continue;
            
            projectile.transform.right = currentDir;

            float distToTarget = Vector3.Distance(context.Origin.position, context.AimLocation);
            distToTarget = Mathf.Max(distToTarget, 10f);
            
            Vector3 targetPos = context.Origin.position + (currentDir * distToTarget);

            IMovementStrategy strategy = _movementConfig.CreateStrategy(
                context.Origin.position,
                projectile.transform.rotation,
                targetPos,
                _baseSpeed
            );

            projectile.Initialize(
                context.Instigator,
                strategy,
                damagePayload,
                _pierceCount,
                _lifetime,
                context.Spawner
            );
        }

        return UniTask.CompletedTask;
    }
}