using System.Collections.Generic;

public interface IAbility
{
    string Name { get; }
    ActionIdentifier ActionId { get; }

    string AnimationTriggerName { get; }
    bool IsPriority { get; }
    bool IsInstant { get; }
    float MaxCastDuration { get; }

    IEnumerable<UsageCondition> Conditions { get; }

    IAbilityData CreateData(IStatsProvider statsProvider, StatSystemConfig config);

    void Execute(AbilityContext context, IAbilityData data);
}
