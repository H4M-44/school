using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/EventDatabase")]
public class EventDatabase : ScriptableObject
{
    public List<GameEventDefinition> events = new();
}

[Serializable]
public class GameEventDefinition
{
    public int id;
    public string type;
    public string npcId;
    public int startDialogueId;
    public int endDialogueId;
    public string promptText = "E";
    public bool repeatable;
}
