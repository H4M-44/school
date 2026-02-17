using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/NpcLocationDatabase")]
public class NpcLocationDatabase : ScriptableObject
{
    public List<NpcLocationSet> sets = new();
}

[Serializable]
public class NpcLocationSet
{
    public int id; // 20001
    public string note;
    public List<NpcSlotEvent> slotEvents = new(); // A~E
}

[Serializable]
public class NpcSlotEvent
{
    public string slot;   // "A" "B" "C" "D" "E"
    public int eventId;   // you can use dialogueId as MVP
}
