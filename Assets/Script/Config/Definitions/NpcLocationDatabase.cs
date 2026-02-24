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
    public int id;       // 20001
    public string note;  // 备注
    public List<NpcPlacement> placements = new(); // <-- 用这个，不用 SlotEvents
}

[Serializable]
public class NpcPlacement
{
    public string npcId;     // "NPC_01"
    public string anchorId;  // "A" / "B" / "C"
    public int eventId;      // optional
}