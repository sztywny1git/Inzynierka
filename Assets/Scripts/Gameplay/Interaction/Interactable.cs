using UnityEngine;

public class Interactable : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactText;
    
    [SerializeField] private Interaction interaction;

    public string GetInteractText()
    {
        if (string.IsNullOrEmpty(interactText) && interaction != null)
        {
            return interaction.Label;
        }
        return interactText;
    }

    public void Interact(InteractionContext context)
    {
        if (interaction != null && interaction.IsAvailable(context))
        {
            interaction.Execute(context);
        }
    }
}