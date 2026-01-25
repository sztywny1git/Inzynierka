using UnityEngine;

public class EnemyMeleeAttack : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool debugLogging = false;

    [Header("Stat Definitions")]
    [SerializeField] private StatDefinition damageStat;
    [SerializeField] private StatDefinition attackRangeStat;
    [SerializeField] private StatDefinition attackCooldownStat;

    [Header("Fallback Values")]
    [SerializeField] private float fallbackDamage = 10f;
    [SerializeField] private float fallbackAttackRange = 1f;
    [SerializeField] private float fallbackAttackCooldownSeconds = 1.2f;

    [Header("Origin")]
    [SerializeField] private Transform attackOrigin;

    private IStatsProvider _stats;
    private float _nextAttackTime;
    
    // ZMIANA: Przechowujemy cel, żeby "pamiętać" kogo uderzamy w momencie Animation Eventu
    private Transform _currentAggroTarget; 

    public float AttackRange => GetStatValue(attackRangeStat, fallbackAttackRange);
    private float Damage => GetStatValue(damageStat, fallbackDamage);
    private float AttackCooldownSeconds => GetStatValue(attackCooldownStat, fallbackAttackCooldownSeconds);

    private Vector2 AttackOriginPosition => attackOrigin != null ? (Vector2)attackOrigin.position : (Vector2)transform.position;

    private void Awake()
    {
        if (attackOrigin == null) attackOrigin = transform;
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

        return Vector2.Distance(origin, target.position);
    }

    // KROK 1: Ta metoda jest wywoływana przez Brain/State
    // Służy tylko do rozpoczęcia procedury (ustawienie cooldownu, zapamiętanie celu)
    public bool TryAttack(Transform target)
    {
        if (target == null) return false;
        
        if (!CanAttackNow()) return false;

        float dist = GetDistanceToTarget(target);
        if (dist > AttackRange) return false;

        var otherEnemy = target.GetComponentInParent<EnemyBrain>();
        if (otherEnemy != null) return false;

        // Ustawiamy cooldown
        _nextAttackTime = Time.time + AttackCooldownSeconds;

        // ZMIANA: Zapamiętujemy cel, ale NIE zadajemy obrażeń tutaj.
        _currentAggroTarget = target;

        // Zwracamy true -> Brain odegra animację -> Animacja wywoła OnAttackImpact()
        return true; 
    }

    // KROK 2: Ta metoda musi być Publiczna. Będzie wywołana przez Animation Event.
    public void OnAttackImpact()
    {
        // Jeśli cel zniknął w trakcie zamachu (np. został zniszczony), przerywamy
        if (_currentAggroTarget == null) return;

        // Opcjonalnie: Sprawdzamy, czy cel nadal jest w zasięgu + mały margines
        // (Gracz mógł zrobić uskok w ostatniej chwili)
        float dist = GetDistanceToTarget(_currentAggroTarget);
        if (dist > AttackRange + 0.5f)
        {
            if (debugLogging) Debug.Log($"[EnemyMeleeAttack] Missed! Target moved out of range during animation.");
            return;
        }

        var damageable = _currentAggroTarget.GetComponentInChildren<IDamageable>();
        if (damageable != null)
        {
            float dmg = Damage;
            var damageData = new DamageData(dmg, false, gameObject, AttackOriginPosition);
            damageable.TakeDamage(damageData);

            if (debugLogging) Debug.Log($"[EnemyMeleeAttack] Animation Event HIT! Dealt {dmg} damage.");
        }
        
        // Czyścimy cel po ataku
        _currentAggroTarget = null;
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