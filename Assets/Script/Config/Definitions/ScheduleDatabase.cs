using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/ScheduleDatabase")]
public class ScheduleDatabase : ScriptableObject
{
    public List<ScheduleDay> days = new();
}

[Serializable]
public class ScheduleDay
{
    public int id;         // 1001
    public int dayNumber;  // 1,2...
    public List<TimeBlock> blocks = new(); // A~F
}

[Serializable]
public class TimeBlock
{
    public string time;         // "10:00" or "10:15"
    public string name;         // 早上/中午...
    public int npcLocationId;   // 20001
    public int startDialogueId; // 900001 (optional)
    public int endDialogueId;   // optional
}
