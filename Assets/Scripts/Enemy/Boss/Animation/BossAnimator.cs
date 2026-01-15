using UnityEngine;

/// <summary>
/// Abstract base class for boss animators.
/// Each boss type should derive from this and define its specific animation parameters.
/// </summary>
public abstract class BossAnimator : MonoBehaviour, IBossAnimator
{
    [Header("Base Animator Settings")]
    [SerializeField] protected Animator animator;
    
    [Header("Common Animation Parameters")]
    [SerializeField] protected string idleTrigger = "idle";
    [SerializeField] protected string walkBool = "isWalking";
    [SerializeField] protected string hitTrigger = "hit";
    [SerializeField] protected string deathTrigger = "death";
    [SerializeField] protected string phaseTransitionTrigger = "phaseTransition";
    
    // Cached hash values for common animations
    protected int IdleHash;
    protected int WalkHash;
    protected int HitHash;
    protected int DeathHash;
    protected int PhaseTransitionHash;
    
    /// <summary>
    /// The current phase the boss is in (affects animation behavior).
    /// </summary>
    public int CurrentPhase { get; private set; } = 1;
    
    protected virtual void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        CacheCommonAnimationHashes();
        CacheAnimationHashes();
    }
    
    private void CacheCommonAnimationHashes()
    {
        IdleHash = Animator.StringToHash(idleTrigger);
        WalkHash = Animator.StringToHash(walkBool);
        HitHash = Animator.StringToHash(hitTrigger);
        DeathHash = Animator.StringToHash(deathTrigger);
        PhaseTransitionHash = Animator.StringToHash(phaseTransitionTrigger);
    }
    
    /// <summary>
    /// Override to cache boss-specific animation parameter hashes.
    /// </summary>
    protected abstract void CacheAnimationHashes();
    
    /// <summary>
    /// Set the current phase for phase-dependent animations.
    /// </summary>
    public virtual void SetPhase(int phase)
    {
        CurrentPhase = phase;
        OnPhaseChanged(phase);
    }
    
    /// <summary>
    /// Called when the phase changes. Override to handle phase-specific animation logic.
    /// </summary>
    protected virtual void OnPhaseChanged(int newPhase) { }
    
    #region Common Animations
    
    public virtual void PlayIdle()
    {
        if (animator == null) return;
        animator.SetTrigger(IdleHash);
    }
    
    public virtual void SetWalking(bool isWalking)
    {
        if (animator == null) return;
        animator.SetBool(WalkHash, isWalking);
    }
    
    public virtual void PlayHit()
    {
        if (animator == null) return;
        animator.SetTrigger(HitHash);
    }
    
    public virtual void PlayDeath()
    {
        if (animator == null) return;
        animator.SetTrigger(DeathHash);
    }
    
    public virtual void PlayPhaseTransition()
    {
        if (animator == null) return;
        animator.SetTrigger(PhaseTransitionHash);
    }
    
    #endregion
    
    #region Abstract Attack Methods (to be implemented by each boss)
    
    /// <summary>
    /// Play the primary attack animation. Implementation varies per boss.
    /// </summary>
    public abstract void PlayPrimaryAttack();
    
    /// <summary>
    /// Play the secondary attack animation. Implementation varies per boss.
    /// </summary>
    public abstract void PlaySecondaryAttack();
    
    /// <summary>
    /// Play a special attack animation. Implementation varies per boss.
    /// </summary>
    public abstract void PlaySpecialAttack();
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Check if any animation is currently playing.
    /// </summary>
    public bool IsAnimationPlaying(string animationName)
    {
        if (animator == null) return false;
        var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName(animationName);
    }
    
    /// <summary>
    /// Get the normalized time of the current animation (0-1).
    /// </summary>
    public float GetCurrentAnimationProgress()
    {
        if (animator == null) return 0f;
        var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.normalizedTime % 1f;
    }
    
    /// <summary>
    /// Check if the current animation has completed.
    /// </summary>
    public bool HasCurrentAnimationCompleted()
    {
        if (animator == null) return true;
        var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.normalizedTime >= 1f;
    }
    
    /// <summary>
    /// Set animator speed (useful for slow-motion effects).
    /// </summary>
    public void SetAnimatorSpeed(float speed)
    {
        if (animator == null) return;
        animator.speed = speed;
    }
    
    #endregion
}
