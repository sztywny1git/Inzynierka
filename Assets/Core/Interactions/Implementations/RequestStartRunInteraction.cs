using UnityEngine;
using VContainer;

[CreateAssetMenu(fileName = "RequestStartRunInteraction", menuName = "Interaction/Request Start Run")]
public class RequestStartRunInteraction : Interaction
{
    public override void Execute(InteractionContext context)
    {
        context.DIContainer.Resolve<GameplayEventBus>().RequestBeginRun();
    }
}