using System.Collections.Generic;
using UnityEngine;

public class NpcDirector : MonoBehaviour
{
    [SerializeField] private NpcAnchorRegistry anchors;

    private readonly Dictionary<string, NpcActor> _npcById = new();

    private void Awake()
    {
        if (anchors == null)
            anchors = FindFirstObjectByType<NpcAnchorRegistry>();

        if (anchors == null)
            Debug.LogError("NpcAnchorRegistry not found in scene!");

        BuildNpcCache();
    }

    private void BuildNpcCache()
    {
        _npcById.Clear();

        var npcs = FindObjectsByType<NpcActor>(FindObjectsSortMode.None);
        foreach (var npc in npcs)
        {
            if (string.IsNullOrWhiteSpace(npc.npcId)) continue;

            var key = npc.npcId.Trim();
            if (_npcById.ContainsKey(key))
            {
                Debug.LogError($"Duplicate npcId in scene: {key}", npc);
                continue;
            }

            _npcById.Add(key, npc);
        }

        Debug.Log($"NpcDirector cached NPCs: {_npcById.Count}");
    }

    private NpcActor FindNpcById(string npcId)
    {
        if (string.IsNullOrWhiteSpace(npcId)) return null;
        _npcById.TryGetValue(npcId.Trim(), out var npc);
        return npc;
    }

    public void ApplyNpcLocation(int npcLocationId)
    {
        Debug.Log($"[NpcDirector] ApplyNpcLocation id={npcLocationId}");

        var set = ConfigQuery.GetNpcLocationSet(npcLocationId);
        if (set == null)
        {
            Debug.LogError($"NpcLocationSet not found: {npcLocationId}");
            return;
        }

        if (anchors == null)
        {
            Debug.LogError("NpcAnchorRegistry is null");
            return;
        }

        // IMPORTANT: your NpcLocationSet must have "placements"
        foreach (var p in set.placements)
        {
            if (string.IsNullOrWhiteSpace(p.npcId) || string.IsNullOrWhiteSpace(p.anchorId))
                continue;

            var npc = FindNpcById(p.npcId);
            if (npc == null)
            {
                Debug.LogWarning($"NPC not found in scene: {p.npcId}");
                continue;
            }

            var anchor = anchors.Get(p.anchorId);
            if (anchor == null)
            {
                Debug.LogWarning($"Anchor not found: {p.anchorId}");
                continue;
            }

            npc.transform.SetPositionAndRotation(anchor.position, anchor.rotation);
        }
    }
}