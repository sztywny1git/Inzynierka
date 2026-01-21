using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Conditions/Cooldown")]
public class CooldownCondition : UsageCondition
{
    [SerializeField] private float _baseDuration;

    public override bool CanBeUsed(AbilityContext context)
    {
        if (context.CooldownProvider == null || context.ActionId == null) return true;
        return !context.CooldownProvider.IsOnCooldown(context.ActionId);
    }

    public override void OnUse(AbilityContext context)
    {
        if (context.CooldownProvider != null && context.ActionId != null)
        {
            context.CooldownProvider.PutOnCooldown(context.ActionId, _baseDuration);
        }
    }
}