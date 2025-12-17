using UnityEngine;

[CreateAssetMenu(fileName = "ResourceCostCondition", menuName = "Abilities/Conditions/Resource Cost")]
public class ResourceCostCondition : UsageCondition
{
    [SerializeField] private float cost;

    public override bool CanBeUsed(AbilityContext context)
    {
        return context.ResourceSystem != null && context.ResourceSystem.CanAfford(cost);
    }

    public override void OnUse(AbilityContext context)
    {
        context.ResourceSystem?.Consume(cost);
    }
}