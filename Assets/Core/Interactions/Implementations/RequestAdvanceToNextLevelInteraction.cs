using UnityEngine;
using VContainer;

[CreateAssetMenu(fileName = "RequestAdvanceInteraction", menuName = "Interaction/Request Advance To Next Level")]
public class RequestAdvanceToNextLevelInteraction : Interaction
{
    public override void Execute(InteractionContext context)
    {
        context.DIContainer.Resolve<GameplayEventBus>().RequestAdvanceToNextLevel();
    }
}