using WorldTime;
using UnityEngine;

public class DailyTimeController : MonoBehaviour
{
    [SerializeField] private WorldTime.WorldTime worldTime;
    [SerializeField] private NpcDirector npcDirector;
    [SerializeField] private DialogueUIController dialogueUIController;

    [SerializeField] private int currentDay = 1;
    [SerializeField] private int currentBlockIndex = -1;

    private bool isWaitingForDialogueFinish = false;

    private void Awake()
    {
        if (npcDirector == null)
            npcDirector = FindFirstObjectByType<NpcDirector>();

        if (worldTime == null)
            worldTime = FindFirstObjectByType<WorldTime.WorldTime>();

        if (dialogueUIController == null)
            dialogueUIController = FindFirstObjectByType<DialogueUIController>();

        if (npcDirector == null)
            Debug.LogError("NpcDirector not found in scene! Make sure it exists and is enabled.");

        if (worldTime == null)
            Debug.LogError("WorldTime not found in scene! Make sure it exists and is enabled.");

        if (dialogueUIController == null)
            Debug.LogError("DialogueUIController not found in scene! Make sure it exists and is enabled.");
    }

    private void OnEnable()
    {
        if (dialogueUIController != null)
            dialogueUIController.OnDialogueFinished += HandleDialogueFinished;
    }

    private void OnDisable()
    {
        if (dialogueUIController != null)
            dialogueUIController.OnDialogueFinished -= HandleDialogueFinished;
    }

    public void NextTime()
    {
        if (isWaitingForDialogueFinish)
        {
            Debug.Log("Dialogue is playing. NextTime is blocked.");
            return;
        }

        var day = ConfigQuery.GetScheduleDay(currentDay);
        if (day == null || day.blocks == null || day.blocks.Count == 0)
        {
            Debug.LogError($"No schedule for day {currentDay}");
            return;
        }

        currentBlockIndex++;

        if (currentBlockIndex >= day.blocks.Count)
        {
            currentDay++;
            currentBlockIndex = 0;

            day = ConfigQuery.GetScheduleDay(currentDay);
            if (day == null || day.blocks == null || day.blocks.Count == 0)
            {
                Debug.LogWarning($"No schedule for day {currentDay}. End.");
                return;
            }
        }

        ApplyBlock(day.blocks[currentBlockIndex]);
    }

    private void ApplyBlock(TimeBlock block)
    {
        Debug.Log($"[Day {currentDay}] Enter {block.time} ({block.name}) npcLoc={block.npcLocationId} startDia={block.startDialogueId} endDia={block.endDialogueId}");

        // 1) 时钟跳转
        if (worldTime != null)
            worldTime.SetTimeHHmm(currentDay, block.time);
        else
            Debug.LogError("WorldTime reference not assigned!");

        // 2) NPC 跳转
        if (block.npcLocationId != 0)
        {
            if (npcDirector != null)
                npcDirector.ApplyNpcLocation(block.npcLocationId);
            else
                Debug.LogError("NpcDirector reference not assigned!");
        }

        // 3) 自动剧情
        bool hasDialogue = block.startDialogueId > 0 && block.endDialogueId > 0;

        if (hasDialogue)
        {
            if (dialogueUIController != null)
            {
                isWaitingForDialogueFinish = true;
                dialogueUIController.StartDialogueByIdRange(block.startDialogueId, block.endDialogueId);
            }
            else
            {
                Debug.LogError("DialogueUIController reference not assigned!");
            }
        }
        else
        {
            isWaitingForDialogueFinish = false;
        }
    }

    private void HandleDialogueFinished()
    {
        Debug.Log("Dialogue finished. Auto advancing to next block.");

        isWaitingForDialogueFinish = false;
        NextTime();
    }
}