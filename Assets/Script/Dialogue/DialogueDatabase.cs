using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/DialogueDatabase")]
public class DialogueDatabase : ScriptableObject
{
    public List<DialogueLine> lines = new();
}

[Serializable]
public class DialogueLine
{
    public int dialogueId;     // 900001
    public string time;        // optional: "10:15"
    public string speaker;     // 主角/女主A
    public string text;        // 你好
}
