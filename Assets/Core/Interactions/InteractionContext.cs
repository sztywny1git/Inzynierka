using UnityEngine;
using VContainer;

public readonly struct InteractionContext
{
    public readonly GameObject Initiator;
    public readonly GameObject Target;
    public readonly IObjectResolver DIContainer;

    public InteractionContext(GameObject initiator, GameObject target, IObjectResolver container)
    {
        Initiator = initiator;
        Target = target;
        DIContainer = container;
    }
}