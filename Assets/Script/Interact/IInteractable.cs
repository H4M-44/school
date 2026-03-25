public interface IInteractable
{
    void Interact(PlayerInteraction playerInteraction);
    string GetPromptText();
}