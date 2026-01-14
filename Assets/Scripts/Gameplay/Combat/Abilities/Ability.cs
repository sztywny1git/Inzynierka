using System.Collections.Generic;
using UnityEngine;

public abstract class Ability : ScriptableObject, IAbility 
{
    [SerializeField] private string _name;
    [SerializeField] private ActionIdentifier _actionId;
    [SerializeField] private string _animationTriggerName;
    [SerializeField] private bool _executeImmediately = false;
    [SerializeField] private List<UsageCondition> _conditions = new List<UsageCondition>();

    public string Name => _name;
    public ActionIdentifier ActionId => _actionId;
    public string AnimationTriggerName => _animationTriggerName;
    public bool ExecuteImmediately => _executeImmediately;
    public IEnumerable<UsageCondition> Conditions => _conditions;

    public abstract IAbilityData CreateData(IStatsProvider statsProvider, StatSystemConfig config);
    public abstract void Execute(AbilityContext context, IAbilityData data);
}