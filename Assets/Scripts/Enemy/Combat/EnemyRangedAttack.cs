using UnityEngine;

public class EnemyRangedAttack : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool debugLogging = false;

    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;

    [Header("Attack Settings")]
    [SerializeField] private float damage = 8f;
    [SerializeField] private float attackRange = 7f;
    [SerializeField] private float attackCooldownSeconds = 1.25f;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float projectileLifetimeSeconds = 3f;

    [Header("Origin")]
    [SerializeField] private Transform shootOrigin;

    private float _nextAttackTime;

    public float AttackRange => attackRange;

    private void Awake()
    {
        if (shootOrigin == null) shootOrigin = transform;
        _nextAttackTime = 0f;
    }

    public bool CanAttackNow()
    {
        return Time.time >= _nextAttackTime;
    }

    public bool TryAttack(Transform target)
    {
        if (projectilePrefab == null)
        {
            if (debugLogging)
            {
                Debug.LogWarning($"[EnemyRangedAttack] '{name}' TryAttack failed: projectilePrefab=null.", this);
            }
            return false;
        }
        if (target == null)
        {
            if (debugLogging)
            {
                Debug.Log($"[EnemyRangedAttack] '{name}' TryAttack failed: target=null.", this);
            }
            return false;
        }
        if (!CanAttackNow()) return false;

        float range = AttackRange;
        float dist = Vector2.Distance(shootOrigin.position, target.position);
        if (dist > range)
        {
            if (debugLogging)
            {
                Debug.Log($"[EnemyRangedAttack] '{name}' TryAttack skip: target '{target.name}' out of range. dist={dist:0.###} range={range:0.###}", this);
            }
            return false;
        }

        Vector2 dir = (target.position - shootOrigin.position);
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;
        dir.Normalize();

        float dmg = Damage;
        float speed = ProjectileSpeed;
        float cooldown = AttackCooldownSeconds;

        var instance = Instantiate(projectilePrefab, shootOrigin.position, Quaternion.identity);

        var enemyProjectile = instance.GetComponent<EnemyProjectile>() ?? instance.GetComponentInChildren<EnemyProjectile>();
        if (enemyProjectile != null)
        {
            enemyProjectile.Initialize(dir, dmg, speed, projectileLifetimeSeconds, instance, gameObject);
        }
        else
        {
            var rb = instance.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = dir * speed;
            }

            Destroy(instance, projectileLifetimeSeconds);

            if (debugLogging)
            {
                Debug.LogWarning($"[EnemyRangedAttack] '{name}' projectile prefab '{projectilePrefab.name}' has no EnemyProjectile component (root or children). It will not deal damage unless it has its own damage script.", this);
            }
        }

        _nextAttackTime = Time.time + cooldown;

        if (debugLogging)
        {
            Debug.Log($"[EnemyRangedAttack] '{name}' SHOT at '{target.name}'. damage={dmg:0.###} dist={dist:0.###} cooldown={cooldown:0.###}", this);
        }
        return true;
    }
}
