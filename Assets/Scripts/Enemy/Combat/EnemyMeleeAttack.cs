using UnityEngine;

public class EnemyMeleeAttack : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool debugLogging = false;

    [Header("Stat Definitions (optional - uses CharacterStats if assigned)")]
    [SerializeField] private StatDefinition damageStat;
    [SerializeField] private StatDefinition attackRangeStat;
    [SerializeField] private StatDefinition attackCooldownStat;

    [Header("Fallback Values (used if stat not found)")]
    [SerializeField] private float fallbackDamage = 10f;
    [SerializeField] private float fallbackAttackRange = 1f;
    [SerializeField] private float fallbackAttackCooldownSeconds = 1.2f;

    [Header("Origin")]
    [SerializeField] private Transform attackOrigin;

    private IStatsProvider _stats;
    private float _nextAttackTime;
    private float _nextCooldownLogTime;

    public float AttackRange => GetStatValue(attackRangeStat, fallbackAttackRange);
    
    private float Damage => GetStatValue(damageStat, fallbackDamage);
    private float AttackCooldownSeconds => GetStatValue(attackCooldownStat, fallbackAttackCooldownSeconds);

    private Vector2 AttackOriginPosition => attackOrigin != null ? (Vector2)attackOrigin.position : (Vector2)transform.position;

    private void Awake()
    {
        if (attackOrigin == null) attackOrigin = transform;
        _stats = GetComponent<IStatsProvider>();
        _nextAttackTime = 0f;
        _nextCooldownLogTime = 0f;
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

    public float GetDistanceToTarget(Transform target)
    {
        if (target == null) return float.PositiveInfinity;

        Vector2 origin = AttackOriginPosition;

        var ownCollider = target.GetComponent<Collider2D>();
        if (ownCollider != null)
        {
            Vector2 closest = ownCollider.ClosestPoint(origin);
            return Vector2.Distance(origin, closest);
        }

        var childColliders = target.GetComponentsInChildren<Collider2D>();
        if (childColliders != null && childColliders.Length > 0)
        {
            float best = float.PositiveInfinity;
            for (int i = 0; i < childColliders.Length; i++)
            {
                var col = childColliders[i];
                if (col == null) continue;
                Vector2 closest = col.ClosestPoint(origin);
                float d = Vector2.Distance(origin, closest);
                if (d < best) best = d;
            }
            return best;
        }

        return Vector2.Distance(origin, target.position);
    }

    public bool TryAttack(Transform target)
    {
        if (target == null)
        {
            if (debugLogging)
            {
                Debug.Log($"[EnemyMeleeAttack] '{name}' TryAttack failed: target=null.", this);
            }
            return false;
        }
        if (!CanAttackNow())
        {
            if (debugLogging && Time.time >= _nextCooldownLogTime)
            {
                float remaining = Mathf.Max(0f, _nextAttackTime - Time.time);
                Debug.Log($"[EnemyMeleeAttack] '{name}' TryAttack blocked by cooldown. remaining={remaining:0.###}s", this);
                _nextCooldownLogTime = Time.time + 0.75f;
            }
            return false;
        }

        float dist = GetDistanceToTarget(target);
        float range = AttackRange;
        if (dist > range)
        {
            if (debugLogging)
            {
                Debug.Log($"[EnemyMeleeAttack] '{name}' TryAttack miss: target '{target.name}' out of range. dist={dist:0.###} range={range:0.###}", this);
            }
            return false;
        }

        var otherEnemy = target.GetComponentInParent<EnemyBrain>();
        if (otherEnemy != null)
        {
            if (debugLogging)
            {
                Debug.Log($"[EnemyMeleeAttack] '{name}' skipping attack on friendly '{target.name}'.", this);
            }
            return false;
        }

        float cooldown = AttackCooldownSeconds;
        _nextAttackTime = Time.time + cooldown;

        var damageable = target.GetComponentInChildren<IDamageable>();
        if (damageable == null)
        {
            if (debugLogging)
            {
                Debug.LogWarning($"[EnemyMeleeAttack] '{name}' attacked '{target.name}' in range but no IDamageable found in children.", this);
            }
            return true;
        }

        float dmg = Damage;
        if (debugLogging)
        {
            Debug.Log($"[EnemyMeleeAttack] '{name}' HIT '{target.name}'. damage={dmg:0.###} dist={dist:0.###} cooldown={cooldown:0.###}", this);
        }
        
        var damageData = new DamageData(dmg, false, gameObject, attackOrigin.position);
        damageable.TakeDamage(damageData);
        return true;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        var origin = attackOrigin != null ? attackOrigin.position : transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin, AttackRange);
    }
#endif
}
