using System;
using UnityEngine;

/// <summary>
/// Base controller for all boss enemies.
/// Manages the hierarchical state machine and phase transitions.
/// </summary>
[RequireComponent(typeof(Health))]
public abstract class BossController : MonoBehaviour, IDamageable
{
    [Header("Boss Settings")]
    [SerializeField] protected float phase2HealthThreshold = 0.5f;
    [SerializeField] protected float phaseTransitionDuration = 2f;
    [SerializeField] protected bool isInvulnerableDuringTransition = true;
    
    [Header("Target Detection")]
    [SerializeField] protected float detectionRange = 15f;
    [SerializeField] protected LayerMask playerLayer;
    
    [Header("Debug")]
    [SerializeField] protected bool debugLogging = false;
    
    protected HierarchicalStateMachine HFSM;
    protected BossContext Context;
    protected BossAnimator BossAnimator;
    protected Health Health;
    
    // Phase states
    protected BossPhaseState Phase1State;
    protected BossPhaseState Phase2State;
    protected BossPhaseTransitionState PhaseTransitionState;
    
    private bool _isTransitioning;
    private bool _isDead;
    private bool _hasTransitionedToPhase2;
    private bool _isInvulnerable;
    
    public event Action<int> OnPhaseChanged;
    public event Action OnBossDeath;
    
    public bool IsTransitioning => _isTransitioning;
    public bool IsDead => _isDead;
    public int CurrentPhase => Context?.CurrentPhase ?? 1;
    
    public void TakeDamage(DamageData damageData)
    {
        if (_isDead) return;
        if (_isInvulnerable) return;
        
        Health?.TakeDamage(damageData);
    }
    
    protected virtual void Awake()
    {
        HFSM = new HierarchicalStateMachine();
        Health = GetComponent<Health>();
        BossAnimator = GetComponent<BossAnimator>();
        
        if (BossAnimator == null)
        {
            Debug.LogError($"[BossController] No BossAnimator found on {name}. Please add one.", this);
        }
    }
    
    protected virtual void Start()
    {
        InitializeContext();
        InitializePhaseStates();
        SubscribeToEvents();
        
        // Start in Phase 1
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
    
    #region Initialization
    
    private void InitializeContext()
    {
        Context = new BossContext(this, BossAnimator, Health);
    }
    
    /// <summary>
    /// Initialize phase states. Must be implemented by derived classes.
    /// </summary>
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
    
    #endregion
    
    #region Phase Management
    
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
        
        // Create transition state if needed
        if (PhaseTransitionState == null)
        {
            PhaseTransitionState = new BossPhaseTransitionState(Context, phaseTransitionDuration, targetPhase);
            PhaseTransitionState.OnTransitionComplete += () => CompletePhaseTransition(targetPhase);
        }
        else
        {
            PhaseTransitionState.SetTargetPhase(targetPhase, phaseTransitionDuration);
        }
        
        if (isInvulnerableDuringTransition)
        {
            _isInvulnerable = true;
        }
        
        BossAnimator?.PlayPhaseTransition();
        HFSM.ChangeState(PhaseTransitionState);
    }
    
    private void CompletePhaseTransition(int targetPhase)
    {
        _isTransitioning = false;
        
        Context.SetPhase(targetPhase);
        BossAnimator?.SetPhase(targetPhase);
        
        if (isInvulnerableDuringTransition)
        {
            _isInvulnerable = false;
        }
        
        // Transition to the appropriate phase state
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
    
    #endregion
    
    #region Target Detection
    
    private void UpdateTargetDetection()
    {
        // Find player within detection range
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);
        
        if (hit != null)
        {
            Context.SetTarget(hit.transform);
        }
    }
    
    #endregion
    
    #region Event Handlers
    
    private void HandleDeath()
    {
        if (_isDead) return;
        
        _isDead = true;
        BossAnimator?.PlayDeath();
        HFSM.Clear();
        
        OnBossDeath?.Invoke();
        
        if (debugLogging)
        {
            Debug.Log($"[BossController] {name} has been defeated!", this);
        }
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
    
    #endregion
    
    #region Debug
    
    protected virtual void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
    
    #endregion
}
