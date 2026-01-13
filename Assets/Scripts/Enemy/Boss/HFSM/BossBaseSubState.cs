using System;

/// <summary>
/// Base class for sub-states within a boss phase.
/// </summary>
public abstract class BossBaseSubState : IState
{
    protected readonly BossContext Context;
    protected readonly BossPhaseState ParentPhase;
    
    private bool _isActive;
    
    public bool IsActive => _isActive;
    
    /// <summary>
    /// Event fired when this sub-state is complete.
    /// </summary>
    public event Action OnComplete;
    
    protected BossBaseSubState(BossContext context, BossPhaseState parentPhase)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        ParentPhase = parentPhase;
    }
    
    public virtual void Enter()
    {
        _isActive = true;
    }
    
    public virtual void Exit()
    {
        _isActive = false;
    }
    
    public abstract void Tick(float deltaTime);
    
    /// <summary>
    /// Call this when the sub-state has completed its action.
    /// </summary>
    protected void Complete()
    {
        OnComplete?.Invoke();
        ParentPhase?.OnSubStateComplete();
    }
}
