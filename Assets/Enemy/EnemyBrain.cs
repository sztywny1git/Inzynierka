using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
[RequireComponent(typeof(EnemyTargetProvider))]
[RequireComponent(typeof(EnemyMeleeAttack))]
public sealed class EnemyBrain : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool debugLogging = false;

    [Header("State Distances")]
    [SerializeField] private float chaseDistance = 5f;
    [SerializeField] private float attackDistance = 1.2f;
    [SerializeField] private float rangedAttackDistance = 6f;
    [SerializeField] private float rangedPreferredMinDistance = 2.5f;
    [SerializeField] private float giveUpDistance = 8f;

    [Header("Idle/Patrol")]
    [SerializeField] private float idleSeconds = 0.5f;
    [SerializeField] private float patrolArriveDistance = 0.25f;
    [SerializeField] private float patrolRepathSeconds = 1.0f;
    [SerializeField] private bool useEnhancedPatrol = true;
    [SerializeField] private EnemyPatrolBehaviorConfig patrolBehaviorConfig;

    [Header("Smart Combat")]
    [SerializeField] private bool useSmartCombat = true;

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
    private ThreatDetector _threatDetector;
    private Health _health;
    private IPatrolPointProvider _patrolProvider;

    private EnemyIdleState _idle;
    private IState _patrol;
    private EnemyChaseState _chase;
    private EnemyAttackState _attack;
    private EnemyRangedAttackState _rangedAttack;
    private EnemySmartCombatState _smartCombat;

    private bool _initialized;
    private IState _lastLoggedState;
    
    private float _lastHealth;
    private float _lastMaxHealth;
    private float _nextAllowedHitAnimTime;

    public bool DebugLogging => debugLogging;
    
    // ZMIANA: Publiczny dostęp dla histerezy w SmartCombatState
    public float ChaseDistance => chaseDistance; 

    private void Awake()
    {
        _fsm = new StateMachine();

        _movement = GetComponent<EnemyMovement>();
        _targetProvider = GetComponent<EnemyTargetProvider>();
        _melee = GetComponent<EnemyMeleeAttack>();
        _ranged = GetComponent<EnemyRangedAttack>();
        _anim = GetComponent<EnemyAnimator>();
        
        if (_anim == null) _anim = gameObject.AddComponent<EnemyAnimator>();

        if (useSmartCombat)
        {
            _threatDetector = GetComponent<ThreatDetector>();
            if (_threatDetector == null) _threatDetector = gameObject.AddComponent<ThreatDetector>();
        }

        if (_movement == null || _targetProvider == null || _melee == null)
        {
            enabled = false;
            return;
        }

        ClampConfiguredDistancesToCombatComponents();
        _ctx = new EnemyContext(this, _movement, _targetProvider, _melee);
        BuildStates();
        _initialized = true;
    }

    private void OnEnable()
    {
        if (!_initialized) return;

        _nextAllowedHitAnimTime = 0f;
        
        _health = GetComponent<Health>() ?? GetComponentInChildren<Health>();
        
        if (_health != null)
        {
            _health.OnHealthChanged += HandleHealthChanged;
            _lastHealth = _health.CurrentHealth;
            _lastMaxHealth = _health.MaxHealth;
        }

        if (_fsm.Current == null && _idle != null)
        {
            _fsm.ChangeState(_idle);
        }
    }

    private void OnDisable()
    {
        if (_health != null)
        {
            _health.OnHealthChanged -= HandleHealthChanged;
        }
    }

    private void Update()
    {
        if (!_initialized) return;

        if (debugLogging)
        {
            var current = _fsm.Current;
            if (!ReferenceEquals(current, _lastLoggedState))
            {
                Debug.Log($"[EnemyBrain] State: {_lastLoggedState?.GetType().Name} -> {current?.GetType().Name}", this);
                _lastLoggedState = current;
            }
        }

        _fsm.Tick(Time.deltaTime);
    }

    private void HandleHealthChanged(float current, float max)
    {
        bool maxUnchanged = Mathf.Abs(max - _lastMaxHealth) < 0.001f;
        
        if (maxUnchanged && current < _lastHealth && current > 0)
        {
            TriggerHitAnimIfAllowed();
        }

        _lastHealth = current;
        _lastMaxHealth = max;
    }

    private void TriggerHitAnimIfAllowed()
    {
        if (_anim == null || Time.time < _nextAllowedHitAnimTime) return;

        _anim.TriggerHit();
        _nextAllowedHitAnimTime = Time.time + Mathf.Max(0f, hitAnimMinIntervalSeconds);
    }

    private void ClampConfiguredDistancesToCombatComponents()
    {
        chaseDistance = Mathf.Max(0f, chaseDistance);
        giveUpDistance = Mathf.Max(0.1f, giveUpDistance);

        if (_melee != null && (attackDistance <= 0f || attackDistance > _melee.AttackRange))
        {
            attackDistance = _melee.AttackRange;
        }

        if (_ranged != null)
        {
            if (rangedAttackDistance <= 0f) rangedAttackDistance = _ranged.AttackRange;
        }
        else
        {
            rangedAttackDistance = 0f;
        }
        rangedPreferredMinDistance = Mathf.Max(0f, rangedPreferredMinDistance);
    }

    private void BuildStates()
    {
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

        if (useSmartCombat)
        {
            _smartCombat = new EnemySmartCombatState(_ctx, _fsm, _threatDetector, _chase, _patrol, _melee != null ? _melee.AttackRange : attackDistance);
        }

        if (_ranged != null)
        {
            _rangedAttack = new EnemyRangedAttackState(_ctx, _fsm, _ranged, rangedAttackDistance, rangedPreferredMinDistance, _attack, _chase);
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

        float dist = _melee != null ? _melee.GetDistanceToTarget(target) : Vector2.Distance(transform.position, target.position);

        if (useSmartCombat && _smartCombat != null && dist <= chaseDistance)
        {
            desired = _smartCombat;
            return !ReferenceEquals(desired, current);
        }

        if (dist <= attackDistance)
        {
            desired = _attack;
            return !ReferenceEquals(desired, current);
        }

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