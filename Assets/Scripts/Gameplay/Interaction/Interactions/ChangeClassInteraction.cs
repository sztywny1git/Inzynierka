using UnityEngine;
using VContainer;

[CreateAssetMenu(fileName = "ChangeClassInteraction", menuName = "Interaction/Change Class")]
public class ChangeClassInteraction : Interaction
{
    [SerializeField] private CharacterDefinition classToAssign;

    public override void Execute(InteractionContext context)
    {
        var eventBus = context.DIContainer.Resolve<GameplayEventBus>();
        eventBus.InvokeClassSelected(classToAssign);
    }
}