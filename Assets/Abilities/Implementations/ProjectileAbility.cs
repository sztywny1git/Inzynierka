using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Projectile")]
public class ProjectileAbility : DamageAbility
{
    [Header("Behavior")]
    [SerializeField] private ProjectileMovementConfig _movementConfig;
    [SerializeField] private LayerMask _obstacleMask;
    [SerializeField] private float _wallOffset = 0.05f;

    [Header("Projectile Settings")]
    [SerializeField] private Projectile _projectilePrefab;
    [SerializeField] private float _baseSpeed = 20f;
    [SerializeField] private float _lifetime = 5f;
    [SerializeField] private float _spreadAngle = 15f;
    [SerializeField] private int _pierceCount = 0;
    [SerializeField] private float _projectileRadius = 0.25f;

    public override void Execute(AbilityContext context, AbilitySnapshot snapshot)
    {
        if (_movementConfig == null || _projectilePrefab == null) return;

        Vector3 aimDiff = context.AimLocation - context.Origin.position;
        aimDiff.z = 0;
        Vector3 aimDir = aimDiff.normalized;

        if (aimDir == Vector3.zero) aimDir = Vector3.right;

        Vector3 spawnOrigin = context.Origin.position;
        Vector3 instigatorPos = context.Instigator.transform.position;

        RaycastHit2D hit = Physics2D.Linecast(instigatorPos, spawnOrigin, _obstacleMask);
        
        if (hit.collider != null)
        {
            float safeDistance = _projectileRadius + _wallOffset;
            spawnOrigin = Vector3.MoveTowards(hit.point, instigatorPos, safeDistance);
        }

        int count = AttackCount;
        
        float startAngle = (count > 1) ? -_spreadAngle / 2f : 0f;
        float angleStep = (count > 1) ? _spreadAngle / (count - 1) : 0f;

        for (int i = 0; i < count; i++)
        {
            DamageData damagePayload = CalculateDamage(context, snapshot);

            float currentAngleOffset = startAngle + (angleStep * i);
            Vector3 currentDir = Quaternion.Euler(0, 0, currentAngleOffset) * aimDir;

            Projectile projectile = context.Spawner.Spawn(_projectilePrefab, spawnOrigin, Quaternion.identity);

            if (projectile != null)
            {
                projectile.transform.right = currentDir;

                float distToTarget = Vector3.Distance(spawnOrigin, context.AimLocation);
                distToTarget = Mathf.Max(distToTarget, 10f);
                
                Vector3 targetPos = spawnOrigin + (currentDir * distToTarget);

                projectile.Initialize(
                    context.Instigator,
                    _movementConfig,
                    targetPos,
                    _baseSpeed,
                    damagePayload,
                    _pierceCount,
                    _lifetime,
                    context.Spawner
                );
            }
        }
    }
}