using UnityEngine;

/// <summary>
/// Sub-state for the Summoner's second melee area attack.
/// This is a stronger, slower attack with larger area.
/// </summary>
public class SummonerAttack2SubState : BossBaseSubState
{
    private readonly SummonerBossController _bossController;
    private float _attackTimer;
    private bool _hasDoneArea;
    private const float DamageDelay = 0.5f; // Slightly longer windup
    private const float AttackDuration = 1.0f;
    
    public SummonerAttack2SubState(BossContext context, BossPhaseState parentPhase, 
        SummonerBossController controller) 
        : base(context, parentPhase)
    {
        _bossController = controller;
    }
    
    public override void Enter()
    {
        base.Enter();
        _attackTimer = 0f;
        _hasDoneArea = false;
        
        // Face target
        FaceTarget();
        
        // Show attack indicator
        if (_bossController.ShowAttackIndicators && _bossController.AttackIndicator != null)
        {
            _bossController.AttackIndicator.ShowCircleIndicator(
                _bossController.Attack2Radius, 
                DamageDelay
            );
        }
        
        // Play attack2 animation
        if (Context.Animator is SummonerBossAnimator summonerAnim)
        {
            summonerAnim.PlayAttack2();
        }
        else
        {
            Context.Animator?.PlaySecondaryAttack();
        }
    }
    
    public override void Tick(float deltaTime)
    {
        _attackTimer += deltaTime;
        
        // Apply damage at the right moment in the animation
        if (!_hasDoneArea && _attackTimer >= DamageDelay)
        {
            _hasDoneArea = true;
            
            Debug.Log($"[SummonerBoss] Attack2 - Area damage! Damage: {_bossController.Attack2Damage}, Radius: {_bossController.Attack2Radius}");
            
            // Hide indicator when damage lands
            _bossController.AttackIndicator?.HideIndicator();
            
            _bossController.PerformAreaDamage(
                _bossController.Attack2Damage, 
                _bossController.Attack2Radius
            );
        }
        
        // Complete after animation duration
        if (_attackTimer >= AttackDuration)
        {
            Complete();
        }
    }
    
    public override void Exit()
    {
        base.Exit();
        // Ensure indicator is hidden when leaving this state
        _bossController.AttackIndicator?.HideIndicator();
    }
    
    private void FaceTarget()
    {
        if (Context.Target == null) return;
        
        Vector3 direction = Context.GetDirectionToTarget();
        if (direction.x != 0)
        {
            Vector3 scale = Context.Transform.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(direction.x);
            Context.Transform.localScale = scale;
        }
    }
}
