using UnityEngine;

/// <summary>
/// Idle sub-state for boss phases.
/// The boss waits and looks for the target.
/// </summary>
public class BossIdleSubState : BossBaseSubState
{
    private readonly float _minIdleTime;
    private readonly float _maxIdleTime;
    private float _idleTimer;
    private float _targetIdleTime;
    
    public BossIdleSubState(BossContext context, BossPhaseState parentPhase, 
        float minIdleTime = 0.5f, float maxIdleTime = 2f) 
        : base(context, parentPhase)
    {
        _minIdleTime = minIdleTime;
        _maxIdleTime = maxIdleTime;
    }
    
    public override void Enter()
    {
        base.Enter();
        _idleTimer = 0f;
        _targetIdleTime = Random.Range(_minIdleTime, _maxIdleTime);
        
        Context.Animator?.PlayIdle();
        Context.Animator?.SetWalking(false);
    }
    
    public override void Exit()
    {
        base.Exit();
    }
    
    public override void Tick(float deltaTime)
    {
        _idleTimer += deltaTime;
        
        // Check if we have a target and idle time is complete
        if (_idleTimer >= _targetIdleTime && Context.Target != null)
        {
            Complete();
        }
    }
}
