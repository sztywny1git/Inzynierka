using UnityEngine;
using System.Collections.Generic;

public abstract class Ability : ScriptableObject
{
    [SerializeField] private string animationTrigger;
    [SerializeField] private List<UsageCondition> conditions;

    public abstract void Execute(AbilityContext context);

    public string AnimationTrigger => animationTrigger;
    public IReadOnlyList<UsageCondition> Conditions => conditions;
}