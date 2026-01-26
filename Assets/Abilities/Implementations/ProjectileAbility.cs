using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Projectile")]
public class ProjectileAbility : DamageAbility
{
    [Header("Behavior")]
    [SerializeField] private ProjectileMovementConfig _movementConfig;
    [SerializeField] private LayerMask _obstacleMask;
    [SerializeField] private float _wallOffset = 0.1f;

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

        Collider2D instigatorCol = context.Instigator.GetComponent<Collider2D>();
        Vector3 instigatorCenter = instigatorCol != null ? instigatorCol.bounds.center : context.Instigator.transform.position;
        
        Vector3 physicalMuzzlePos = context.Origin.position;
        Vector3 spawnOrigin = physicalMuzzlePos;

        bool isPhysicalMuzzleBlocked = Physics2D.OverlapCircle(physicalMuzzlePos, _projectileRadius * 0.9f, _obstacleMask);

        if (isPhysicalMuzzleBlocked)
        {
            Vector3 aimDirFromCenter = (context.AimLocation - instigatorCenter).normalized;
            if (aimDirFromCenter == Vector3.zero) aimDirFromCenter = Vector3.right;

            float distanceToMuzzle = Vector3.Distance(instigatorCenter, physicalMuzzlePos);
            float checkDistance = Mathf.Max(distanceToMuzzle, _projectileRadius + _wallOffset);

            RaycastHit2D hit = Physics2D.CircleCast(instigatorCenter, _projectileRadius * 0.9f, aimDirFromCenter, checkDistance, _obstacleMask);

            if (hit.collider != null)
            {
                spawnOrigin = hit.centroid - ((Vector2)aimDirFromCenter * _wallOffset);
            }
            else
            {
                spawnOrigin = instigatorCenter + (aimDirFromCenter * distanceToMuzzle);
            }

            int safetyChecks = 5;
            for (int k = 0; k < safetyChecks; k++)
            {
                if (Physics2D.OverlapCircle(spawnOrigin, _projectileRadius * 0.9f, _obstacleMask))
                {
                    spawnOrigin = Vector3.Lerp(spawnOrigin, instigatorCenter, 0.25f);
                }
                else
                {
                    break;
                }
            }

            if (Physics2D.OverlapCircle(spawnOrigin, _projectileRadius * 0.5f, _obstacleMask))
            {
                spawnOrigin = instigatorCenter;
            }
        }

        Vector3 aimDiff = context.AimLocation - spawnOrigin;
        aimDiff.z = 0;
        Vector3 aimDir = aimDiff.normalized;

        if (aimDir == Vector3.zero) aimDir = Vector3.right;

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