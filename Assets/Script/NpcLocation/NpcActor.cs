using UnityEngine;

public class NpcActor : MonoBehaviour, IInteractable
{
    public string npcId; // e.g. "NPC_01"

    [SerializeField] private float interactionRadius = 2f;

    private int activeEventId;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
            return;

        col.isTrigger = true;

        if (col is SphereCollider sphere && sphere.radius < interactionRadius)
            sphere.radius = interactionRadius;
    }

    public void SetActiveEvent(int eventId)
    {
        activeEventId = eventId;
        Debug.Log($"[NpcActor] {npcId} active event set to {activeEventId}", this);
    }

    public void ClearActiveEvent()
    {
        activeEventId = 0;
    }

    public void Interact(PlayerInteraction playerInteraction)
    {
        if (activeEventId <= 0)
            return;

        GameEventDefinition eventDefinition = ConfigQuery.GetEvent(activeEventId);
        if (eventDefinition == null)
        {
            Debug.LogWarning($"Event not found: {activeEventId}", this);
            return;
        }

        if (!IsChatEvent(eventDefinition))
        {
            Debug.LogWarning($"Unsupported event type: {eventDefinition.type}", this);
            return;
        }

        if (!string.IsNullOrWhiteSpace(eventDefinition.npcId) && eventDefinition.npcId.Trim() != npcId.Trim())
        {
            Debug.LogWarning($"Event {eventDefinition.id} is assigned to {eventDefinition.npcId}, but this NPC is {npcId}.", this);
            return;
        }

        DialogueUIController dialogue = FindFirstObjectByType<DialogueUIController>();
        if (dialogue == null)
        {
            Debug.LogError("DialogueUIController not found in scene.");
            return;
        }

        dialogue.StartDialogueByIdRange(eventDefinition.startDialogueId, eventDefinition.endDialogueId, DialogueSource.Interaction);

        if (!eventDefinition.repeatable)
            ClearActiveEvent();
    }

    public string GetPromptText()
    {
        if (activeEventId <= 0)
            return "";

        GameEventDefinition eventDefinition = ConfigQuery.GetEvent(activeEventId);
        if (eventDefinition == null || string.IsNullOrWhiteSpace(eventDefinition.promptText))
            return "E";

        return eventDefinition.promptText;
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

    private bool IsChatEvent(GameEventDefinition eventDefinition)
    {
        string type = eventDefinition.type;
        return string.IsNullOrWhiteSpace(type) ||
               type.Trim().Contains(":") ||
               type.Trim().ToLowerInvariant() == "chat" ||
               type.Trim() == "聊天";
    }

    private void TrySetPlayerInteractable(Collider other)
    {
        if (activeEventId <= 0)
            return;

        PlayerInteraction playerInteraction = other.GetComponentInParent<PlayerInteraction>();
        if (playerInteraction != null)
            playerInteraction.SetInteractable(this);
    }
}
