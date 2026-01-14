using UnityEngine;

public abstract class Interaction : ScriptableObject
{
    public string Label;

    public virtual bool IsAvailable(InteractionContext context) => true;
    public abstract void Execute(InteractionContext context);
}