using UnityEngine;

[DisallowMultipleComponent]
public class DialogueInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private int dialogueId;
    [SerializeField] private string promptText = "E";
    [SerializeField] private float autoTriggerRadius = 0.1f;
    [SerializeField] private bool disableAfterInteract;

    private bool hasBeenUsed;

    private void Awake()
    {
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            if (col != null && col.isTrigger)
                return;
        }

        SphereCollider triggerCollider = gameObject.AddComponent<SphereCollider>();
        triggerCollider.isTrigger = true;
        triggerCollider.radius = autoTriggerRadius > 0f ? autoTriggerRadius : 2f;
    }

    public void Interact(PlayerInteraction playerInteraction)
    {
        if (hasBeenUsed && disableAfterInteract)
            return;

        if (dialogueId <= 0)
        {
            Debug.LogWarning($"[{nameof(DialogueInteractable)}] Dialogue ID is not configured.", this);
            return;
        }

        DialogueUIController dialogue = FindFirstObjectByType<DialogueUIController>();
        if (dialogue == null)
        {
            Debug.LogError($"[{nameof(DialogueInteractable)}] DialogueUIController not found in scene.", this);
            return;
        }

        if (dialogue.IsPlaying)
            return;

        dialogue.StartDialogueById(dialogueId, DialogueSource.Interaction);

        if (disableAfterInteract)
        {
            hasBeenUsed = true;
            if (playerInteraction != null)
                playerInteraction.ClearInteractable(this);
        }
    }

    public string GetPromptText()
    {
        if (hasBeenUsed && disableAfterInteract)
            return string.Empty;

        return string.IsNullOrWhiteSpace(promptText) ? "E" : promptText;
    }

    private void OnTriggerEnter(Collider other)
    {
        TrySetPlayerInteractable(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TrySetPlayerInteractable(other);
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerInteraction playerInteraction = other.GetComponentInParent<PlayerInteraction>();
        if (playerInteraction != null)
            playerInteraction.ClearInteractable(this);
    }

    private void TrySetPlayerInteractable(Collider other)
    {
        if (hasBeenUsed && disableAfterInteract)
            return;

        PlayerInteraction playerInteraction = other.GetComponentInParent<PlayerInteraction>();
        if (playerInteraction != null)
            playerInteraction.SetInteractable(this);
    }
}
