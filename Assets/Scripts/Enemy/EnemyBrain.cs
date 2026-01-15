using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
[RequireComponent(typeof(EnemyTargetProvider))]
[RequireComponent(typeof(EnemyMeleeAttack))]
public sealed class EnemyBrain : MonoBehaviour, IDamageable
{
    [Header("Debug")]
    [SerializeField] private bool debugLogging = false;

    [Header("State Distances")]
    [SerializeField] private float chaseDistance = 6f;
    [SerializeField] private float attackDistance = 1.0f;
    [SerializeField] private float rangedAttackDistance = 7.0f;
    [SerializeField] private float rangedPreferredMinDistance = 3.0f;
    [SerializeField] private float giveUpDistance = 10f;

    [Header("Idle/Patrol")]
    [SerializeField] private float idleSeconds = 0.5f;
    [SerializeField] private float patrolArriveDistance = 0.25f;
    [SerializeField] private float patrolRepathSeconds = 1.0f;
    [SerializeField] private bool useEnhancedPatrol = true;
    [SerializeField] private EnemyPatrolBehaviorConfig patrolBehaviorConfig;

    [Header("Death")]
    [SerializeField] private bool destroyOnDeath = true;
    [SerializeField] private float destroyDelaySeconds = 1.5f;

    [Header("Patrol Provider")]
    [SerializeField] private MonoBehaviour patrolPointProviderBehaviour;

    [Header("Animation")]
    [SerializeField] private float hitAnimMinIntervalSeconds = 0.1f;

    private StateMachine _fsm;
    private EnemyContext _ctx;

    private EnemyMovement _movement;
    private EnemyTargetProvider _targetProvider;
    private EnemyMeleeAttack _melee;
    private EnemyRangedAttack _ranged;
    private EnemyAnimator _anim;

    private IPatrolPointProvider _patrolProvider;
    private Health _health;

    // Cached states
    private EnemyIdleState _idle;
    private IState _patrol; // Can be EnemyPatrolState or EnemyEnhancedPatrolState
    private EnemyChaseState _chase;
    private EnemyAttackState _attack;
    private EnemyRangedAttackState _rangedAttack;

    private bool _initialized;
    private bool _isDead;

    private IState _lastLoggedState;

    private float _lastHealth;
    private float _lastMaxHealth;
    private float _nextAllowedHitAnimTime;

    // Relay damage to Health so projectiles/colliders can always find an IDamageable on the enemy root.
    public bool DebugLogging => debugLogging;

    public void TakeDamage(DamageData damageData)
    {
        if (damageData.Amount <= 0f) return;
        if (_isDead) return;

        EnsureHealthCached();
        if (_health == null)
        {
            if (debugLogging)
            {
                Debug.LogWarning($"[EnemyBrain] TakeDamage({damageData.Amount}) but no Health found on '{name}'.", this);
            }
            return;
        }

        _health.TakeDamage(damageData);
    }

    private void Awake()
    {
        _fsm = new StateMachine();

        _movement = GetComponent<EnemyMovement>();
        _targetProvider = GetComponent<EnemyTargetProvider>();
        _melee = GetComponent<EnemyMeleeAttack>();
        _ranged = GetComponent<EnemyRangedAttack>();

        _anim = GetComponent<EnemyAnimator>();
        if (_anim == null)
        {
            // Keep the old behavior (auto-add) but keep it explicit and predictable.
            _anim = gameObject.AddComponent<EnemyAnimator>();
        }

        // Sanity check required dependencies.
        if (_movement == null || _targetProvider == null || _melee == null)
        {
            Debug.LogError(
                $"[EnemyBrain] Missing required components on '{name}'. " +
                $"movement={_movement != null} targetProvider={_targetProvider != null} melee={_melee != null}. Disabling EnemyBrain.",
                this);
            enabled = false;
            return;
        }

        ClampConfiguredDistancesToCombatComponents();

        _ctx = new EnemyContext(this, _movement, _targetProvider, _melee);
      //  _patrolProvider = ResolvePatrolProvider();

        BuildStates();

        _initialized = true;

        if (debugLogging)
        {
            Debug.Log(
                $"[EnemyBrain] Awake OK '{name}'. meleeRange={_melee.AttackRange:0.###} attackDistance={attackDistance:0.###} " +
                $"ranged={_ranged != null} rangedDistance={rangedAttackDistance:0.###}",
                this);
        }
    }

    private void OnEnable()
    {
        if (!_initialized) return;

        _isDead = false;
        _nextAllowedHitAnimTime = 0f;

        EnsureHealthCached();
        SubscribeHealthEvents();

        // Ensure we always have a state after enable.
        if (_fsm.Current == null && _idle != null)
        {
            _fsm.ChangeState(_idle);
        }
    }

    private void OnDisable()
    {
        UnsubscribeHealthEvents();
    }

    private void Update()
    {
        if (!_initialized) return;
        if (_isDead) return;

        if (debugLogging)
        {
            var current = _fsm.Current;
            if (!ReferenceEquals(current, _lastLoggedState))
            {
                string prevName = _lastLoggedState != null ? _lastLoggedState.GetType().Name : "<null>";
                string nextName = current != null ? current.GetType().Name : "<null>";
                Debug.Log($"[EnemyBrain] '{name}' state {prevName} -> {nextName}", this);
                _lastLoggedState = current;
            }
        }

        _fsm.Tick(Time.deltaTime);
    }

    private void EnsureHealthCached()
    {
        if (_health != null) return;
        _health = GetComponent<Health>() ?? GetComponentInChildren<Health>();
        if (_health != null)
        {
            _lastHealth = _health.CurrentHealth;
            _lastMaxHealth = _health.MaxHealth;
        }
    }

    private void SubscribeHealthEvents()
    {
        if (_health == null) return;
        _health.Death -= HandleDeath;
        _health.OnHealthChanged -= HandleHealthChanged;
        _health.Death += HandleDeath;
        _health.OnHealthChanged += HandleHealthChanged;
    }

    private void UnsubscribeHealthEvents()
    {
        if (_health == null) return;
        _health.Death -= HandleDeath;
        _health.OnHealthChanged -= HandleHealthChanged;
    }

    // Called when HP changes. If it decreased, treat it as taking damage (and play hit anim).
    private void HandleHealthChanged(float current, float max)
    {
        if (_isDead) return;

        // Treat HP decrease as damage only if MaxHealth didn't change.
        // MaxHealth changes can also lower CurrentHealth due to % preservation.
        bool maxUnchanged = Mathf.Abs(max - _lastMaxHealth) < 0.001f;
        if (maxUnchanged && current < _lastHealth)
        {
            TriggerHitIfAllowed();
        }

        _lastHealth = current;
        _lastMaxHealth = max;
    }

    public void TriggerHitIfAllowed()
    {
        if (_isDead) return;
        if (_anim == null)
        {
            if (debugLogging)
            {
                Debug.LogWarning($"[EnemyBrain] TriggerHit requested but EnemyAnimator is missing on '{name}'.", this);
            }
            return;
        }
        if (Time.time < _nextAllowedHitAnimTime) return;

        _anim.TriggerHit();
        _nextAllowedHitAnimTime = Time.time + Mathf.Max(0f, hitAnimMinIntervalSeconds);
    }

    private void HandleDeath()
    {
        if (_isDead) return;
        _isDead = true;

        _anim?.TriggerDie();
        _movement?.Stop();

        foreach (var col in GetComponentsInChildren<Collider2D>())
        {
            col.enabled = false;
        }

        // Stop ticking AI immediately.
        enabled = false;

        if (destroyOnDeath)
        {
            Destroy(gameObject, Mathf.Max(0f, destroyDelaySeconds));
        }
    }

    private void ClampConfiguredDistancesToCombatComponents()
    {
        chaseDistance = Mathf.Max(0f, chaseDistance);
        giveUpDistance = Mathf.Max(0.1f, giveUpDistance);

        if (_melee != null)
        {
            // Prevent the "attack state dead-zone": entering attack farther than we can actually hit.
            if (attackDistance <= 0f || attackDistance > _melee.AttackRange)
            {
                attackDistance = _melee.AttackRange;
            }
        }

        if (_ranged != null)
        {
            if (rangedAttackDistance <= 0f)
            {
                rangedAttackDistance = _ranged.AttackRange;
            }
        }
        else
        {
            // No ranged component => ensure we never prefer ranged.
            rangedAttackDistance = 0f;
        }

        rangedPreferredMinDistance = Mathf.Max(0f, rangedPreferredMinDistance);
    }

    private IPatrolPointProvider ResolvePatrolProvider()
    {
        if (patrolPointProviderBehaviour is IPatrolPointProvider explicitProvider)
        {
            return explicitProvider;
        }

        var fromComponents = GetComponent<IPatrolPointProvider>();
        if (fromComponents != null)
        {
            return fromComponents;
        }

        return gameObject.AddComponent<DungeonFloorPatrolPointProvider>();
    }

    private void BuildStates()
    {
        // Create patrol state - use enhanced version if enabled
        if (useEnhancedPatrol)
        {
            var config = patrolBehaviorConfig ?? new EnemyPatrolBehaviorConfig();
            config.ArriveDistance = patrolArriveDistance;
            config.RepathSeconds = patrolRepathSeconds;
            _patrol = new EnemyEnhancedPatrolState(_ctx, _fsm, _patrolProvider, config);
        }
        else
        {
            _patrol = new EnemyPatrolState(_ctx, _fsm, _patrolProvider, patrolArriveDistance, patrolRepathSeconds);
        }

        _chase = new EnemyChaseState(_ctx, _fsm, giveUpDistance, _patrol);
        _attack = new EnemyAttackState(_ctx, _fsm, attackDistance, _chase);

        if (_ranged != null)
        {
            _rangedAttack = new EnemyRangedAttackState(
                _ctx,
                _fsm,
                _ranged,
                rangedAttackDistance,
                rangedPreferredMinDistance,
                _attack,
                _chase);
        }
        else
        {
            _rangedAttack = null;
        }

        _idle = new EnemyIdleState(_ctx, _fsm, idleSeconds, _patrol);
        _fsm.ChangeState(_idle);
    }

    public bool TryGetDesiredCombatState(IState current, out IState desired)
    {
        desired = null;
        if (!_initialized) return false;

        var target = _ctx.TargetProvider.Target;
        if (target == null) return false;

        float dist = _melee != null
            ? _melee.GetDistanceToTarget(target)
            : Vector2.Distance(transform.position, target.position);

        // Prefer melee when close.
        if (dist <= attackDistance)
        {
            desired = _attack;
            return !ReferenceEquals(desired, current);
        }

        // Prefer ranged only when not in melee range.
        if (_rangedAttack != null && dist <= rangedAttackDistance)
        {
            desired = _rangedAttack;
            return !ReferenceEquals(desired, current);
        }

        if (dist <= chaseDistance)
        {
            desired = _chase;
            return !ReferenceEquals(desired, current);
        }

        return false;
    }
}
