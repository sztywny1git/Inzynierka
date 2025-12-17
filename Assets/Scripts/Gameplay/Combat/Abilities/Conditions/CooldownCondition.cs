using UnityEngine;
using VContainer;

[CreateAssetMenu(fileName = "CooldownCondition", menuName = "Abilities/Conditions/Cooldown")]
public class CooldownCondition : UsageCondition
{
    [SerializeField] private ActionIdentifier actionIdentifier;
    [SerializeField] private float cooldown;

    public override bool CanBeUsed(AbilityContext context)
    {
        if (actionIdentifier == null) return true;
        
        var cooldownProvider = context.DIContainer.Resolve<ICooldownProvider>();
        int ownerId = context.Owner.GetInstanceID();
        return !cooldownProvider.IsOnCooldown(ownerId, actionIdentifier);
    }

    public override void OnUse(AbilityContext context)
    {
        if (actionIdentifier == null) return;

        var cooldownProvider = context.DIContainer.Resolve<ICooldownProvider>();
        int ownerId = context.Owner.GetInstanceID();
        cooldownProvider.StartCooldown(ownerId, actionIdentifier, cooldown);
    }
}