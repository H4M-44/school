using UnityEngine;
using System;

public class DailyTimeController : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private NpcDirector npcDirector;
    [SerializeField] private DialogueUIController dialogueUIController;

    private bool isWaitingForDialogueFinish = false;

    private GameStateManager GameState => GameStateManager.Instance;
    private WorldTime TimeService => WorldTime.Instance;

    private void Awake()
    {
    if (npcDirector == null)
        npcDirector = FindFirstObjectByType<NpcDirector>();

    if (dialogueUIController == null)
        dialogueUIController = FindFirstObjectByType<DialogueUIController>();

    if (npcDirector == null)
        Debug.LogError("[DailyTimeController] NpcDirector not found in scene.");

    if (dialogueUIController == null)
        Debug.LogError("[DailyTimeController] DialogueUIController not found in scene.");
    }

    private void Start()
    {
        if (GameState == null)
            Debug.LogError("[DailyTimeController] GameStateManager.Instance is missing. Make sure GameBootstrap exists in the startup scene.");

        if (TimeService == null)
            Debug.LogError("[DailyTimeController] WorldTime.Instance is missing. Make sure GameBootstrap exists in the startup scene.");
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
            Debug.Log("[DailyTimeController] Dialogue is playing. NextTime is blocked.");
            return;
        }

        if (GameState == null)
        {
            Debug.LogError("[DailyTimeController] Cannot advance time because GameStateManager is missing.");
            return;
        }

        int currentDay = GameState.CurrentDay;
        int currentBlockIndex = GameState.CurrentBlockIndex;

        var day = ConfigQuery.GetScheduleDay(currentDay);
        if (day == null || day.blocks == null || day.blocks.Count == 0)
        {
            Debug.LogError($"[DailyTimeController] No schedule for day {currentDay}.");
            return;
        }

        int nextBlockIndex = currentBlockIndex + 1;
        int nextDay = currentDay;

        if (nextBlockIndex >= day.blocks.Count)
        {
            nextDay++;
            nextBlockIndex = 0;

            day = ConfigQuery.GetScheduleDay(nextDay);
            if (day == null || day.blocks == null || day.blocks.Count == 0)
            {
                Debug.LogWarning($"[DailyTimeController] No schedule for day {nextDay}. End.");
                return;
            }
        }

        GameState.SetDayAndBlock(nextDay, nextBlockIndex);

        ApplyBlock(day.blocks[nextBlockIndex], nextDay);
    }

    private void ApplyBlock(TimeBlock block, int day)
    {
        Debug.Log($"[Day {day}] Enter {block.time} ({block.name}) npcLoc={block.npcLocationId} startDia={block.startDialogueId} endDia={block.endDialogueId}");

        // 1) Set world time from block.time
        if (TimeService != null)
        {
            if (TryParseBlockTime(block.time, out int hour, out int minute))
            {
                TimeService.SetTime(hour, minute);
            }
            else
            {
                Debug.LogError($"[DailyTimeController] Failed to parse block time: {block.time}");
            }
        }
        else
        {
            Debug.LogError("[DailyTimeController] WorldTime.Instance is missing.");
        }

        // 2) Move NPCs
        if (block.npcLocationId != 0)
        {
            if (npcDirector != null)
            {
                npcDirector.ApplyNpcLocation(block.npcLocationId);
            }
            else
            {
                Debug.LogError("[DailyTimeController] NpcDirector reference not assigned.");
            }
        }

        // 3) Auto dialogue
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
                Debug.LogError("[DailyTimeController] DialogueUIController reference not assigned.");
                isWaitingForDialogueFinish = false;
            }
        }
        else
        {
            isWaitingForDialogueFinish = false;
        }
    }

    private void HandleDialogueFinished()
    {
        Debug.Log("[DailyTimeController] Dialogue finished. Auto advancing to next block.");

        isWaitingForDialogueFinish = false;
        NextTime();
    }

    private bool TryParseBlockTime(string timeText, out int hour, out int minute)
    {
        hour = 0;
        minute = 0;

        if (string.IsNullOrWhiteSpace(timeText))
            return false;

        string[] parts = timeText.Split(':');
        if (parts.Length != 2)
            return false;

        if (!int.TryParse(parts[0], out hour))
            return false;

        if (!int.TryParse(parts[1], out minute))
            return false;

        if (hour < 0 || hour > 23)
            return false;

        if (minute < 0 || minute > 59)
            return false;

        return true;
    }
}