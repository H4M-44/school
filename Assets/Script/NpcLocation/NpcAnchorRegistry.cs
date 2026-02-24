using System.Collections.Generic;
using UnityEngine;

public class NpcAnchorRegistry : MonoBehaviour
{
    private Dictionary<string, Transform> _map;

    private void Awake()
    {
        _map = new Dictionary<string, Transform>();

        var anchors = FindObjectsByType<NpcAnchor>(FindObjectsSortMode.None);
        foreach (var a in anchors)
        {
            if (string.IsNullOrWhiteSpace(a.anchorId)) continue;

            if (_map.ContainsKey(a.anchorId))
            {
                Debug.LogError($"Duplicate anchorId: {a.anchorId}", a);
                continue;
            }

            _map.Add(a.anchorId.Trim(), a.transform);
        }
    }

    public Transform Get(string anchorId)
    {
        if (_map == null) return null;
        if (string.IsNullOrWhiteSpace(anchorId)) return null;

        _map.TryGetValue(anchorId.Trim(), out var t);
        return t;
    }
}