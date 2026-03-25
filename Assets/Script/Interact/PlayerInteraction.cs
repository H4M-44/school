using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private InteractionPromptUI promptUI;

    private IInteractable currentInteractable;

    private void Awake()
    {
        if (promptUI == null)
        {
            promptUI = GetComponentInChildren<InteractionPromptUI>();
        }
    }

    private void Update()
    {
        if (currentInteractable != null && Input.GetKeyDown(KeyCode.E))
        {
            currentInteractable.Interact(this);
        }
    }

    public void SetInteractable(IInteractable interactable)
    {
        currentInteractable = interactable;

        if (currentInteractable != null)
        {
            ShowPrompt(currentInteractable.GetPromptText());
        }
        else
        {
            HidePrompt();
        }
    }

    public void ClearInteractable(IInteractable interactable)
    {
        if (currentInteractable == interactable)
        {
            currentInteractable = null;
            HidePrompt();
        }
    }

    public void ShowPrompt(string text)
    {
        if (promptUI != null)
        {
            promptUI.Show(text);
        }
    }

    public void HidePrompt()
    {
        if (promptUI != null)
        {
            promptUI.Hide();
        }
    }
}