public interface IInteractable
{
    string GetInteractText();
    void Interact(InteractionContext context);
}