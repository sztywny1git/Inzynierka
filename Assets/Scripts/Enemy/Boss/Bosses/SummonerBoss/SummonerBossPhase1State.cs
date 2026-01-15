using UnityEngine;

/// <summary>
/// Summoner Boss Phase 1 state.
/// Alternates between melee attacks and summoning minions.
/// More methodical, gives player time to react.
/// </summary>
public class SummonerBossPhase1State : BossPhaseState
{
    private readonly SummonerBossController _bossController;
    
    // Sub-states
    private BossIdleSubState _idleState;
    private BossChaseSubState _chaseState;
    private SummonerAttack1SubState _attack1State;
    private SummonerAttack2SubState _attack2State;
    private SummonerSummonSubState _summonState;
    private SummonerRingAttackSubState _ringAttackState;
    
    // Attack pattern tracking
    private int _attackCounter;
    private float _lastSummonTime;
    private float _lastRingAttackTime;
    private const int AttacksBeforeSummon = 3;
    private const float RingAttackCooldown = 10f;
    
    public SummonerBossPhase1State(BossContext context, SummonerBossController controller) 
        : base(context)
    {
        _bossController = controller;
    }
    
    protected override void OnPhaseEnter()
    {
        Debug.Log("[SummonerBoss] Phase 1 - The ritual begins...");
        Context.Animator?.SetPhase(1);
        _attackCounter = 0;
        _lastSummonTime = -_bossController.SummonCooldown; // Allow immediate summon
        _lastRingAttackTime = -RingAttackCooldown;
    }
    
    protected override void OnPhaseExit()
    {
        Debug.Log("[SummonerBoss] Exiting Phase 1");
    }
    
    protected override void InitializeSubStates()
    {
        _idleState = new BossIdleSubState(Context, this, 1f, 2f);
        _chaseState = new BossChaseSubState(Context, this, _bossController.Phase1MoveSpeed, _bossController.MeleeAttackRange);
        _attack1State = new SummonerAttack1SubState(Context, this, _bossController);
        _attack2State = new SummonerAttack2SubState(Context, this, _bossController);
        _summonState = new SummonerSummonSubState(Context, this, _bossController, _bossController.Phase1SummonCount);
        _ringAttackState = new SummonerRingAttackSubState(Context, this, _bossController, 
            _bossController.Phase1RingProjectiles, 
            _bossController.RingProjectileSpeed, 
            _bossController.RingProjectileDamage);
        
        // Wire up state transitions
        _idleState.OnComplete += OnIdleComplete;
        _chaseState.OnComplete += OnChaseComplete;
        _attack1State.OnComplete += OnAttackComplete;
        _attack2State.OnComplete += OnAttackComplete;
        _summonState.OnComplete += OnSummonComplete;
        _ringAttackState.OnComplete += OnRingAttackComplete;
        
        // Start in idle
        ChangeSubState(_idleState);
    }
    
    private void OnIdleComplete()
    {
        // After idle, check if we should do ring attack, summon, or chase
        if (ShouldRingAttack())
        {
            ChangeSubState(_ringAttackState);
        }
        else if (ShouldSummon())
        {
            ChangeSubState(_summonState);
        }
        else
        {
            ChangeSubState(_chaseState);
        }
    }
    
    private void OnChaseComplete()
    {
        // Reached target, choose an attack
        if (Context.IsTargetInRange(_bossController.MeleeAttackRange))
        {
            ChooseAttack();
        }
        else
        {
            ChangeSubState(_idleState);
        }
    }
    
    private void ChooseAttack()
    {
        // Alternate between attack1 and attack2
        if (_attackCounter % 2 == 0)
        {
            ChangeSubState(_attack1State);
        }
        else
        {
            ChangeSubState(_attack2State);
        }
        _attackCounter++;
    }
    
    private void OnAttackComplete()
    {
        // After attack, go back to idle
        ChangeSubState(_idleState);
    }
    
    private void OnSummonComplete()
    {
        _lastSummonTime = Time.time;
        ChangeSubState(_idleState);
    }
    
    private void OnRingAttackComplete()
    {
        _lastRingAttackTime = Time.time;
        ChangeSubState(_idleState);
    }
    
    private bool ShouldRingAttack()
    {
        return Time.time - _lastRingAttackTime >= RingAttackCooldown;
    }
    
    private bool ShouldSummon()
    {
        bool cooldownReady = Time.time - _lastSummonTime >= _bossController.SummonCooldown;
        bool enoughAttacks = _attackCounter >= AttacksBeforeSummon;
        
        if (cooldownReady && enoughAttacks)
        {
            _attackCounter = 0;
            return true;
        }
        return false;
    }
    
    protected override void CheckPhaseTransition()
    {
        // Phase transition is handled by BossController based on health
    }
    
    public override void OnSubStateComplete()
    {
        // Handled by individual callbacks
    }
}
