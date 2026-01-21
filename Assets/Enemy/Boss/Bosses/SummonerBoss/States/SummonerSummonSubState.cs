using UnityEngine;

/// <summary>
/// Sub-state for the Summoner's summon ability.
/// Spawns minions to aid the boss.
/// </summary>
public class SummonerSummonSubState : BossBaseSubState
{
    private readonly SummonerBossController _bossController;
    private readonly int _summonCount;
    private float _summonTimer;
    private bool _hasSpawned;
    private const float SpawnDelay = 0.6f; // Time into animation when spawn occurs
    private const float SummonDuration = 1.2f;
    
    public SummonerSummonSubState(BossContext context, BossPhaseState parentPhase, 
        SummonerBossController controller, int summonCount) 
        : base(context, parentPhase)
    {
        _bossController = controller;
        _summonCount = summonCount;
    }
    
    public override void Enter()
    {
        base.Enter();
        _summonTimer = 0f;
        _hasSpawned = false;
        
        // Play summon animation
        if (Context.Animator is SummonerBossAnimator summonerAnim)
        {
            summonerAnim.PlaySummon();
        }
        else
        {
            Context.Animator?.PlaySpecialAttack();
        }
        
        Debug.Log("[SummonerBoss] Summoning minions...");
    }
    
    public override void Tick(float deltaTime)
    {
        _summonTimer += deltaTime;
        
        // Spawn minions at the right moment in the animation
        if (!_hasSpawned && _summonTimer >= SpawnDelay)
        {
            _hasSpawned = true;
            _bossController.SpawnMinions(_summonCount);
        }
        
        // Complete after animation duration
        if (_summonTimer >= SummonDuration)
        {
            Complete();
        }
    }
}
