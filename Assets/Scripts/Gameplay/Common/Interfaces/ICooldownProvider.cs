public interface ICooldownProvider
{
    bool IsOnCooldown(ActionIdentifier actionId);
    float GetRemainingDuration(ActionIdentifier actionId);
    void PutOnCooldown(ActionIdentifier actionId, float baseDuration);
    void ResetCooldown(ActionIdentifier actionId);
}