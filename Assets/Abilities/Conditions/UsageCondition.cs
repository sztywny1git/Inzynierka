using UnityEngine;

public abstract class UsageCondition : ScriptableObject
{
    public abstract bool CanBeUsed(AbilityContext context);
    public virtual void OnUse(AbilityContext context) { }
}