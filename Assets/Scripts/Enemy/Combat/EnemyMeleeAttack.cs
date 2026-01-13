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
    [SerializeField] private float fallbackAttackRange = 0.8f;
    [SerializeField] private float fallbackAttackCooldownSeconds = 1.0f;

    [Header("Origin")]
    [SerializeField] private Transform attackOrigin;

    private IStatsProvider _stats;
    private float _nextAttackTime;
    private float _nextCooldownLogTime;

    public float AttackRange => GetStatValue(attackRangeStat, fallbackAttackRange);
    
    private float Damage => GetStatValue(damageStat, fallbackDamage);
    private float AttackCooldownSeconds => GetStatValue(attackCooldownStat, fallbackAttackCooldownSeconds);

    public Vector2 AttackOriginPosition
    {
        get
        {
            if (attackOrigin != null) return attackOrigin.position;
            return transform.position;
        }
    }

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

        // Prefer distance to the target's collider surface (not pivot), so melee AI doesn't
        // get stuck failing range checks due to Rigidbody2D/collider separation.
        var ownCollider = target.GetComponent<Collider2D>();
        if (ownCollider != null)
        {
            Vector2 closest = ownCollider.ClosestPoint(origin);
            return Vector2.Distance(origin, closest);
        }

        // Fallback: try children colliders (common when collider is on a child object).
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

        // Last resort: pivot distance.
        return Vector2.Distance(origin, target.position);
    }

    public bool IsTargetInRange(Transform target)
    {
        return GetDistanceToTarget(target) <= AttackRange;
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

        // Execute attack if target is in range.
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

        // Consume cooldown even if target currently has no damage component wired.
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
        damageable.TakeDamage(dmg);
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
