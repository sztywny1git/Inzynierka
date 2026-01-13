using UnityEngine;

/// <summary>
/// Sub-state for the Summoner's first melee area attack.
/// </summary>
public class SummonerAttack1SubState : BossBaseSubState
{
    private readonly SummonerBossController _bossController;
    private float _attackTimer;
    private bool _hasDoneArea;
    private const float DamageDelay = 0.4f; // Time into animation when damage occurs
    private const float AttackDuration = 0.8f;
    
    public SummonerAttack1SubState(BossContext context, BossPhaseState parentPhase, 
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
                _bossController.Attack1Radius, 
                DamageDelay
            );
        }
        
        // Play attack animation
        if (Context.Animator is SummonerBossAnimator summonerAnim)
        {
            summonerAnim.PlayAttack();
        }
        else
        {
            Context.Animator?.PlayPrimaryAttack();
        }
    }
    
    public override void Tick(float deltaTime)
    {
        _attackTimer += deltaTime;
        
        // Apply damage at the right moment in the animation
        if (!_hasDoneArea && _attackTimer >= DamageDelay)
        {
            _hasDoneArea = true;
            
            // Hide indicator when damage lands
            _bossController.AttackIndicator?.HideIndicator();
            
            _bossController.PerformAreaDamage(
                _bossController.Attack1Damage, 
                _bossController.Attack1Radius
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
