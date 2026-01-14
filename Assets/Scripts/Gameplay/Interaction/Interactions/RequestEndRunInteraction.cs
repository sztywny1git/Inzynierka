using UnityEngine;
using VContainer;

[CreateAssetMenu(fileName = "PublishRunWonInteraction", menuName = "Interaction/Publish Run Won")]
public class PublishRunWonInteraction : Interaction
{
    public override void Execute(InteractionContext context)
    {
        context.DIContainer.Resolve<GameplayEventBus>().InvokeRunWon();
    }
}