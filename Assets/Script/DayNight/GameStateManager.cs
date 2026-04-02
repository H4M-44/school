using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [Header("Initial State")]
    [SerializeField] private int initialDay = 1;
    [SerializeField] private int initialBlockIndex = -1;

    public int CurrentDay { get; private set; }
    public int CurrentBlockIndex { get; private set; }

    // Later for scene entrance / spawn system
    public string PendingSpawnId { get; private set; }
    public string LastDoorId { get; private set; }

    public event Action<int> DayChanged;
    public event Action<int> BlockIndexChanged;

    private bool _initialized = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeIfNeeded();
    }

    private void InitializeIfNeeded()
    {
        if (_initialized)
            return;

        CurrentDay = initialDay;
        CurrentBlockIndex = initialBlockIndex;
        PendingSpawnId = string.Empty;
        LastDoorId = string.Empty;

        _initialized = true;
    }

    public void SetDay(int day)
    {
        if (day < 1)
        {
            Debug.LogError($"[GameStateManager] Invalid day: {day}");
            return;
        }

        if (CurrentDay == day)
            return;

        CurrentDay = day;
        DayChanged?.Invoke(CurrentDay);
    }

    public void SetBlockIndex(int blockIndex)
    {
        if (CurrentBlockIndex == blockIndex)
            return;

        CurrentBlockIndex = blockIndex;
        BlockIndexChanged?.Invoke(CurrentBlockIndex);
    }

    public void SetDayAndBlock(int day, int blockIndex)
    {
        bool dayChanged = CurrentDay != day;
        bool blockChanged = CurrentBlockIndex != blockIndex;

        CurrentDay = day;
        CurrentBlockIndex = blockIndex;

        if (dayChanged)
            DayChanged?.Invoke(CurrentDay);

        if (blockChanged)
            BlockIndexChanged?.Invoke(CurrentBlockIndex);
    }

    public void SetPendingSpawn(string spawnId, string doorId = "")
    {
        PendingSpawnId = spawnId ?? string.Empty;
        LastDoorId = doorId ?? string.Empty;
    }

    public void ClearPendingSpawn()
    {
        PendingSpawnId = string.Empty;
    }

    public void ResetState()
    {
        CurrentDay = initialDay;
        CurrentBlockIndex = initialBlockIndex;
        PendingSpawnId = string.Empty;
        LastDoorId = string.Empty;

        DayChanged?.Invoke(CurrentDay);
        BlockIndexChanged?.Invoke(CurrentBlockIndex);
    }
}