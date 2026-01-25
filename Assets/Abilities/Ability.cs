using System.Collections.Generic;
using UnityEngine;

public abstract class Ability : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private ActionIdentifier _actionId;
    [SerializeField] private string _animationTriggerName;
    
    [Header("Behavior Flags")]
    [SerializeField] private bool _isPriority;

    [Header("Safety")]
    [SerializeField] private float _maxCastDuration = 2.0f;
    [SerializeField] private List<UsageCondition> _conditions = new List<UsageCondition>();

    public string Name => _name;
    public ActionIdentifier ActionId => _actionId;
    public string AnimationTriggerName => _animationTriggerName;
    public bool IsPriority => _isPriority;
    public float MaxCastDuration => _maxCastDuration;
    public IEnumerable<UsageCondition> Conditions => _conditions;

    public abstract void Execute(AbilityContext context, AbilitySnapshot snapshot);
}