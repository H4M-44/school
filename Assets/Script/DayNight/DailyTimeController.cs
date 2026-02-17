using WorldTime;
using UnityEngine;

public class DailyTimeController : MonoBehaviour
{
    [SerializeField] private WorldTime.WorldTime worldTime;

    [SerializeField] private int currentDay = 1;
    [SerializeField] private int currentBlockIndex = -1;

    public void NextTime()
    {
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
        Debug.Log($"[Day {currentDay}] Enter {block.time} ({block.name}) npcLoc={block.npcLocationId} startDia={block.startDialogueId}");

        // IMPORTANT: apply to in-game clock
        if (worldTime != null)
            worldTime.SetTimeHHmm(currentDay, block.time);
        else
            Debug.LogError("WorldTime reference not assigned!");
    }
}
