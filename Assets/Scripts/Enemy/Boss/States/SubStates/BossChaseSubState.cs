using UnityEngine;

/// <summary>
/// Chase sub-state for boss phases.
/// The boss moves toward the target.
/// </summary>
public class BossChaseSubState : BossBaseSubState
{
    private readonly float _moveSpeed;
    private readonly float _arriveDistance;
    
    public BossChaseSubState(BossContext context, BossPhaseState parentPhase,
        float moveSpeed = 3f, float arriveDistance = 2f) 
        : base(context, parentPhase)
    {
        _moveSpeed = moveSpeed;
        _arriveDistance = arriveDistance;
    }
    
    public override void Enter()
    {
        base.Enter();
        Context.Animator?.SetWalking(true);
    }
    
    public override void Exit()
    {
        base.Exit();
        Context.Animator?.SetWalking(false);
    }
    
    public override void Tick(float deltaTime)
    {
        if (Context.Target == null)
        {
            Complete();
            return;
        }
        
        float distance = Context.GetDistanceToTarget();
        
        if (distance <= _arriveDistance)
        {
            Complete();
            return;
        }
        
        // Move toward target
        Vector3 direction = Context.GetDirectionToTarget();
        Context.Transform.position += direction * _moveSpeed * deltaTime;
        
        // Flip sprite based on direction
        if (direction.x != 0)
        {
            Vector3 scale = Context.Transform.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(direction.x);
            Context.Transform.localScale = scale;
        }
    }
}
