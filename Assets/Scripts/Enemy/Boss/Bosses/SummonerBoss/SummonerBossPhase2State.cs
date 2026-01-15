using UnityEngine;

/// <summary>
/// Summoner Boss Phase 2 state.
/// More aggressive, faster attacks, more frequent summons.
/// </summary>
public class SummonerBossPhase2State : BossPhaseState
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
    private const int AttacksBeforeSummon = 2; // Summons more frequently in Phase 2
    private const float RingAttackCooldown = 6f; // Faster ring attacks in Phase 2
    
    public SummonerBossPhase2State(BossContext context, SummonerBossController controller) 
        : base(context)
    {
        _bossController = controller;
    }
    
    protected override void OnPhaseEnter()
    {
        Debug.Log("[SummonerBoss] Phase 2 - The summoner's fury is unleashed!");
        Context.Animator?.SetPhase(2);
        _attackCounter = 0;
        _lastSummonTime = -_bossController.SummonCooldown; // Allow immediate summon
        _lastRingAttackTime = -RingAttackCooldown;
        
        // Immediately summon reinforcements when entering Phase 2
        _bossController.SpawnMinions(_bossController.Phase2SummonCount);
    }
    
    protected override void OnPhaseExit()
    {
        Debug.Log("[SummonerBoss] Exiting Phase 2");
    }
    
    protected override void InitializeSubStates()
    {
        // Phase 2: shorter idle times, faster movement
        _idleState = new BossIdleSubState(Context, this, 0.3f, 0.8f);
        _chaseState = new BossChaseSubState(Context, this, _bossController.Phase2MoveSpeed, _bossController.MeleeAttackRange);
        _attack1State = new SummonerAttack1SubState(Context, this, _bossController);
        _attack2State = new SummonerAttack2SubState(Context, this, _bossController);
        _summonState = new SummonerSummonSubState(Context, this, _bossController, _bossController.Phase2SummonCount);
        _ringAttackState = new SummonerRingAttackSubState(Context, this, _bossController,
            _bossController.Phase2RingProjectiles,
            _bossController.RingProjectileSpeed * 1.2f, // Faster projectiles in Phase 2
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
        // In Phase 2, more likely to use the stronger attack
        float attackChoice = Random.value;
        
        if (attackChoice < 0.4f)
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
        // In Phase 2, sometimes chain attacks
        if (Random.value < 0.3f && Context.IsTargetInRange(_bossController.MeleeAttackRange))
        {
            // Chain attack
            ChooseAttack();
        }
        else
        {
            ChangeSubState(_idleState);
        }
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
        // Phase 2 has reduced cooldown (half)
        float phase2Cooldown = _bossController.SummonCooldown * 0.5f;
        bool cooldownReady = Time.time - _lastSummonTime >= phase2Cooldown;
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
        // Phase 2 is the final phase
    }
    
    public override void OnSubStateComplete()
    {
        // Handled by individual callbacks
    }
}
