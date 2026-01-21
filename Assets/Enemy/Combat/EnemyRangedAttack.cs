using UnityEngine;

public class EnemyRangedAttack : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool debugLogging = false;

    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;

    [Header("Stat Definitions (optional - uses CharacterStats if assigned)")]
    [SerializeField] private StatDefinition damageStat;
    [SerializeField] private StatDefinition attackRangeStat;
    [SerializeField] private StatDefinition attackCooldownStat;
    [SerializeField] private StatDefinition projectileSpeedStat;

    [Header("Fallback Values (used if stat not found)")]
    [SerializeField] private float fallbackDamage = 8f;
    [SerializeField] private float fallbackAttackRange = 7f;
    [SerializeField] private float fallbackAttackCooldownSeconds = 1.25f;
    [SerializeField] private float fallbackProjectileSpeed = 10f;
    [SerializeField] private float projectileLifetimeSeconds = 3f;

    [Header("Origin")]
    [SerializeField] private Transform shootOrigin;

    private IStatsProvider _stats;
    private float _nextAttackTime;

    public float AttackRange => GetStatValue(attackRangeStat, fallbackAttackRange);
    
    private float Damage => GetStatValue(damageStat, fallbackDamage);
    private float AttackCooldownSeconds => GetStatValue(attackCooldownStat, fallbackAttackCooldownSeconds);
    private float ProjectileSpeed => GetStatValue(projectileSpeedStat, fallbackProjectileSpeed);

    private void Awake()
    {
        if (shootOrigin == null) shootOrigin = transform;
        _stats = GetComponent<IStatsProvider>();
        _nextAttackTime = 0f;
    }

    private float GetStatValue(StatDefinition statDef, float fallback)
    {
        if (statDef == null || _stats == null) return fallback;
        var stat = _stats.GetStat(statDef);
        if (stat == null) return fallback;
        float value = stat.FinalValue;
        return value > 0f ? value : fallback;
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

        // Try initialize EnemyProjectile (allow it to be on a child).
        var enemyProjectile = instance.GetComponent<EnemyProjectile>() ?? instance.GetComponentInChildren<EnemyProjectile>();
        if (enemyProjectile != null)
        {
            enemyProjectile.Initialize(dir, dmg, speed, projectileLifetimeSeconds, instance, gameObject);
        }
        else
        {
            // Fallback: attempt to set Rigidbody2D velocity if present.
            var rb = instance.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = dir * speed;
            }

            // Ensure non-scripted projectile prefabs don't pile up forever.
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
