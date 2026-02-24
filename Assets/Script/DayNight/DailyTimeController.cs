using WorldTime;
using UnityEngine;

public class DailyTimeController : MonoBehaviour
{
    [SerializeField] private WorldTime.WorldTime worldTime;
    [SerializeField] private NpcDirector npcDirector;

    [SerializeField] private int currentDay = 1;
    [SerializeField] private int currentBlockIndex = -1;

    private void Awake()
    {
        // 兜底：避免你忘了拖引用导致 null
        if (npcDirector == null)
            npcDirector = FindFirstObjectByType<NpcDirector>();

        if (worldTime == null)
            worldTime = FindFirstObjectByType<WorldTime.WorldTime>();

        if (npcDirector == null)
            Debug.LogError("NpcDirector not found in scene! Make sure it exists and is enabled.");

        if (worldTime == null)
            Debug.LogError("WorldTime not found in scene! Make sure it exists and is enabled.");
    }

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

        // 1) 时钟跳转
        if (worldTime != null)
            worldTime.SetTimeHHmm(currentDay, block.time);
        else
            Debug.LogError("WorldTime reference not assigned!");

        // 2) NPC 跳转（你缺的就是这一段）
        if (block.npcLocationId != 0)
        {
            if (npcDirector != null)
                npcDirector.ApplyNpcLocation(block.npcLocationId);
            else
                Debug.LogError("NpcDirector reference not assigned!");
        }
    }
}