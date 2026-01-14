using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Conditions/Resource Cost")]
public class ResourceCostCondition : UsageCondition
{
    [SerializeField] private float _cost;

    public override bool CanBeUsed(AbilityContext context)
    {
        if (context.ResourceProvider == null) return true;
        return context.ResourceProvider.HasEnough(_cost);
    }

    public override void OnUse(AbilityContext context)
    {
        if (context.ResourceProvider != null)
        {
            context.ResourceProvider.Consume(_cost);
        }
    }
}