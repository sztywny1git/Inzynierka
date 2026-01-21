/// <summary>
/// Interface that defines the contract for boss animators.
/// All boss animators must implement these basic animation capabilities.
/// </summary>
public interface IBossAnimator
{
    /// <summary>
    /// Current phase the boss is in.
    /// </summary>
    int CurrentPhase { get; }
    
    /// <summary>
    /// Set the current phase.
    /// </summary>
    void SetPhase(int phase);
    
    // Common animations
    void PlayIdle();
    void SetWalking(bool isWalking);
    void PlayHit();
    void PlayDeath();
    void PlayPhaseTransition();
    
    // Attack animations (implementation varies per boss)
    void PlayPrimaryAttack();
    void PlaySecondaryAttack();
    void PlaySpecialAttack();
    
    // Utility
    bool IsAnimationPlaying(string animationName);
    float GetCurrentAnimationProgress();
    bool HasCurrentAnimationCompleted();
}
