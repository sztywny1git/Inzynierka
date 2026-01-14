using System.Collections.Generic;

public interface IAbility
{
    string Name { get; }
    ActionIdentifier ActionId { get; }
    IEnumerable<UsageCondition> Conditions { get; }
    string AnimationTriggerName { get; }
    bool ExecuteImmediately { get; }

    IAbilityData CreateData(IStatsProvider statsProvider, StatSystemConfig config);
    void Execute(AbilityContext context, IAbilityData data);
}