using System;
using UnityEngine;

/// <summary>
/// Base class for boss phase states.
/// Each phase contains its own sub-state machine for handling
/// behaviors within that phase (idle, attack patterns, etc.).
/// </summary>
public abstract class BossPhaseState : IHierarchicalState
{
    protected readonly BossContext Context;
    protected readonly StateMachine SubStateMachine;
    
    private bool _isActive;
    
    public IState CurrentSubState => SubStateMachine.Current;
    public bool HasSubStates => SubStateMachine.Current != null;
    public bool IsActive => _isActive;
    
    /// <summary>
    /// Event fired when this phase should transition to the next phase.
    /// </summary>
    public event Action OnPhaseComplete;
    
    protected BossPhaseState(BossContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        SubStateMachine = new StateMachine();
    }
    
    public virtual void Enter()
    {
        _isActive = true;
        OnPhaseEnter();
        InitializeSubStates();
    }
    
    public virtual void Exit()
    {
        _isActive = false;
        OnPhaseExit();
    }
    
    public virtual void Tick(float deltaTime)
    {
        if (!_isActive) return;
        
        SubStateMachine.Tick(deltaTime);
        CheckPhaseTransition();
    }
    
    public virtual void OnSubStateComplete()
    {
        // Override in derived classes to handle sub-state completion
    }
    
    /// <summary>
    /// Called when entering this phase. Override to set up phase-specific logic.
    /// </summary>
    protected abstract void OnPhaseEnter();
    
    /// <summary>
    /// Called when exiting this phase. Override to clean up phase-specific logic.
    /// </summary>
    protected abstract void OnPhaseExit();
    
    /// <summary>
    /// Initialize the sub-states for this phase.
    /// </summary>
    protected abstract void InitializeSubStates();
    
    /// <summary>
    /// Check if this phase should transition to the next phase.
    /// Call TriggerPhaseComplete() when ready to transition.
    /// </summary>
    protected abstract void CheckPhaseTransition();
    
    /// <summary>
    /// Call this to signal that this phase is complete and should transition.
    /// </summary>
    protected void TriggerPhaseComplete()
    {
        OnPhaseComplete?.Invoke();
    }
    
    /// <summary>
    /// Change to a different sub-state within this phase.
    /// </summary>
    protected void ChangeSubState(IState newSubState)
    {
        SubStateMachine.ChangeState(newSubState);
    }
}
