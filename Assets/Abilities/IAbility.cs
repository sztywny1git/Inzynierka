using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public interface IAbility
{
    string Name { get; }
    ActionIdentifier ActionId { get; }

    string AnimationTriggerName { get; }
    bool IsPriority { get; }
    bool IsInstant { get; }
    float MaxCastDuration { get; }

    IEnumerable<UsageCondition> Conditions { get; }

    public abstract UniTask Execute(AbilityContext context, AbilitySnapshot snapshot);
}