using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private InteractionPromptUI promptUI;

    private void Awake()
    {
        if (promptUI == null)
        {
            promptUI = GetComponentInChildren<InteractionPromptUI>();
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