public interface ICooldownProvider
{
    bool IsOnCooldown(int ownerId, ActionIdentifier actionId);
    void StartCooldown(int ownerId, ActionIdentifier actionId, float duration);
}