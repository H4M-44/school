using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string promptText = "Press E";
    [SerializeField] private string targetSceneName;

    public void Interact(PlayerInteraction playerInteraction)
    {
        Debug.Log("Door interacted");

        if (!string.IsNullOrEmpty(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogWarning("Target scene name is empty.");
        }
    }

    public string GetPromptText()
    {
        return promptText;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();
        if (playerInteraction != null)
        {
            playerInteraction.SetInteractable(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();
        if (playerInteraction != null)
        {
            playerInteraction.ClearInteractable(this);
        }
    }
}