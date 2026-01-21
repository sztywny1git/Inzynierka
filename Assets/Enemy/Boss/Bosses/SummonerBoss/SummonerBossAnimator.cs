using UnityEngine;

/// <summary>
/// Animator for the Summoner Boss.
/// Animations: idle, attack, attack2, death, summon
/// </summary>
public class SummonerBossAnimator : BossAnimator
{
    [Header("Summoner Specific Animations")]
    [SerializeField] private string attackTrigger = "attack";
    [SerializeField] private string attack2Trigger = "attack2";
    [SerializeField] private string summonTrigger = "summon";
    
    // Cached hashes
    private int _attackHash;
    private int _attack2Hash;
    private int _summonHash;
    
    protected override void CacheAnimationHashes()
    {
        _attackHash = Animator.StringToHash(attackTrigger);
        _attack2Hash = Animator.StringToHash(attack2Trigger);
        _summonHash = Animator.StringToHash(summonTrigger);
    }
    
    protected override void OnPhaseChanged(int newPhase)
    {
        // Phase 2: Boss becomes more aggressive
        if (newPhase == 2)
        {
            Debug.Log("[SummonerBoss] Entering Phase 2 - Increased aggression!");
        }
    }
    
    #region Attack Implementations
    
    public override void PlayPrimaryAttack()
    {
        PlayAttack();
    }
    
    public override void PlaySecondaryAttack()
    {
        PlayAttack2();
    }
    
    public override void PlaySpecialAttack()
    {
        PlaySummon();
    }
    
    #endregion
    
    #region Summoner Specific Animations
    
    /// <summary>
    /// Play the first melee area attack animation.
    /// </summary>
    public void PlayAttack()
    {
        if (animator == null) return;
        animator.SetTrigger(_attackHash);
    }
    
    /// <summary>
    /// Play the second melee area attack animation.
    /// </summary>
    public void PlayAttack2()
    {
        if (animator == null) return;
        animator.SetTrigger(_attack2Hash);
    }
    
    /// <summary>
    /// Play the summon animation.
    /// </summary>
    public void PlaySummon()
    {
        if (animator == null) return;
        animator.SetTrigger(_summonHash);
    }
    
    #endregion
}
