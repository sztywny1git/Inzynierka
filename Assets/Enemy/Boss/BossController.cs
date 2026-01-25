using System;
using UnityEngine;

[RequireComponent(typeof(Health))]
public abstract class BossController : MonoBehaviour
{
    [Header("Boss Settings")]
    [SerializeField] protected float phase2HealthThreshold = 0.5f;
    [SerializeField] protected float phaseTransitionDuration = 2f;
    [SerializeField] protected bool isInvulnerableDuringTransition = true;
    
    [Header("Rewards")]
    [SerializeField] private GameObject _portalPrefab;
    [SerializeField] private Vector3 _portalOffset = Vector3.zero;
    
    [Header("Target Detection")]
    [SerializeField] protected float detectionRange = 15f;
    [SerializeField] protected LayerMask playerLayer;
    
    [Header("Debug")]
    [SerializeField] protected bool debugLogging = false;
    
    protected HierarchicalStateMachine HFSM;
    protected BossContext Context;
    protected BossAnimator BossAnimator;
    protected Health Health;
    
    protected BossPhaseState Phase1State;
    protected BossPhaseState Phase2State;
    protected BossPhaseTransitionState PhaseTransitionState;
    
    private bool _isTransitioning;
    private bool _isDead;
    private bool _hasTransitionedToPhase2;
    
    public event Action<int> OnPhaseChanged;
    public event Action OnBossDeath;
    
    public bool IsTransitioning => _isTransitioning;
    public bool IsDead => _isDead;
    public int CurrentPhase => Context?.CurrentPhase ?? 1;
    
    protected virtual void Awake()
    {
        HFSM = new HierarchicalStateMachine();
        Health = GetComponent<Health>();
        BossAnimator = GetComponent<BossAnimator>();
        
        if (BossAnimator == null)
        {
            Debug.LogError($"[BossController] No BossAnimator found on {name}.", this);
        }
    }
    
    protected virtual void Start()
    {
        InitializeContext();
        InitializePhaseStates();
        SubscribeToEvents();
        
        HFSM.ChangeState(Phase1State);
        
        if (debugLogging)
        {
            Debug.Log($"[BossController] {name} initialized. Starting in Phase 1.", this);
        }
    }
    
    protected virtual void Update()
    {
        if (_isDead) return;
        
        HFSM.Tick(Time.deltaTime);
        UpdateTargetDetection();
        CheckPhaseTransition();
    }
    
    protected virtual void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    private void InitializeContext()
    {
        Context = new BossContext(this, BossAnimator, Health);
    }
    
    protected abstract void InitializePhaseStates();
    
    private void SubscribeToEvents()
    {
        if (Health != null)
        {
            Health.Death += HandleDeath;
        }
        
        if (Phase1State != null)
        {
            Phase1State.OnPhaseComplete += HandlePhase1Complete;
        }
        
        HFSM.OnStateChanged += HandleStateChanged;
    }
    
    private void UnsubscribeFromEvents()
    {
        if (Health != null)
        {
            Health.Death -= HandleDeath;
        }
        
        if (Phase1State != null)
        {
            Phase1State.OnPhaseComplete -= HandlePhase1Complete;
        }
        
        HFSM.OnStateChanged -= HandleStateChanged;
    }
    
    private void CheckPhaseTransition()
    {
        if (_hasTransitionedToPhase2 || _isTransitioning) return;
        
        float healthPercent = Context.GetHealthPercentage();
        
        if (healthPercent <= phase2HealthThreshold)
        {
            StartPhaseTransition(2);
        }
    }
    
    private void HandlePhase1Complete()
    {
        if (!_hasTransitionedToPhase2)
        {
            StartPhaseTransition(2);
        }
    }
    
    protected void StartPhaseTransition(int targetPhase)
    {
        if (_isTransitioning) return;
        
        _isTransitioning = true;
        
        if (debugLogging)
        {
            Debug.Log($"[BossController] {name} starting transition to Phase {targetPhase}.", this);
        }
        
        if (PhaseTransitionState == null)
        {
            PhaseTransitionState = new BossPhaseTransitionState(Context, phaseTransitionDuration, targetPhase);
            PhaseTransitionState.OnTransitionComplete += () => CompletePhaseTransition(targetPhase);
        }
        else
        {
            PhaseTransitionState.SetTargetPhase(targetPhase, phaseTransitionDuration);
        }
        
        if (isInvulnerableDuringTransition && Health != null)
        {
            Health.SetInvulnerable(true);
        }
        
        BossAnimator?.PlayPhaseTransition();
        HFSM.ChangeState(PhaseTransitionState);
    }
    
    private void CompletePhaseTransition(int targetPhase)
    {
        _isTransitioning = false;
        
        Context.SetPhase(targetPhase);
        BossAnimator?.SetPhase(targetPhase);
        
        if (isInvulnerableDuringTransition && Health != null)
        {
            Health.SetInvulnerable(false);
        }
        
        BossPhaseState targetState = targetPhase switch
        {
            1 => Phase1State,
            2 => Phase2State,
            _ => Phase2State
        };
        
        if (targetPhase == 2)
        {
            _hasTransitionedToPhase2 = true;
        }
        
        HFSM.ChangeState(targetState);
        OnPhaseChanged?.Invoke(targetPhase);
        
        if (debugLogging)
        {
            Debug.Log($"[BossController] {name} transitioned to Phase {targetPhase}.", this);
        }
    }
    
    private void UpdateTargetDetection()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);
        
        if (hit != null)
        {
            Context.SetTarget(hit.transform);
        }
    }
    
    private void HandleDeath()
    {
        if (_isDead) return;
        
        _isDead = true;
        
        enabled = false;
        
        BossAnimator?.PlayDeath();
        HFSM.Clear();
        
        OnBossDeath?.Invoke();
        
        if (debugLogging)
        {
            Debug.Log($"[BossController] {name} has been defeated!", this);
        }

        Destroy(gameObject, 10f);
    }

    public void OnDeathAnimationFinished()
    {
        if (_portalPrefab != null)
        {
            Instantiate(_portalPrefab, transform.position + _portalOffset, Quaternion.identity);
        }

        Destroy(gameObject);
    }
    
    private void HandleStateChanged(IState previousState, IState newState)
    {
        if (debugLogging)
        {
            string prevName = previousState?.GetType().Name ?? "None";
            string newName = newState?.GetType().Name ?? "None";
            Debug.Log($"[BossController] {name} state changed: {prevName} -> {newName}", this);
        }
    }
    
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}