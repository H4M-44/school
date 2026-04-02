using UnityEngine;
using System;

public class WorldTime : MonoBehaviour
{
    public static WorldTime Instance { get; private set; }

    [Header("Initial Time")]
    [SerializeField] private int startHour = 6;
    [SerializeField] private int startMinute = 0;

    public TimeSpan CurrentTime { get; private set; }
    public int CurrentDay => GameStateManager.Instance != null ? GameStateManager.Instance.CurrentDay : 1;

    public event Action<TimeSpan> TimeChanged;
    public event Action<int> DayChanged;

    private bool _initialized = false;
    private const int MinutesInDay = 1440;

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

        CurrentTime = new TimeSpan(startHour, startMinute, 0);
        _initialized = true;
    }

    public void SetTime(TimeSpan newTime)
    {
        int totalMinutes = Mathf.Clamp((int)newTime.TotalMinutes, 0, MinutesInDay - 1);
        CurrentTime = TimeSpan.FromMinutes(totalMinutes);

        TimeChanged?.Invoke(CurrentTime);
    }

    public void SetTime(int hour, int minute)
    {
        SetTime(new TimeSpan(hour, minute, 0));
    }

    public void AdvanceMinutes(int minutes)
    {
        if (minutes == 0)
            return;

        int currentMinutes = (int)CurrentTime.TotalMinutes;
        int newTotal = currentMinutes + minutes;

        while (newTotal >= MinutesInDay)
        {
            newTotal -= MinutesInDay;
            IncrementDay();
        }

        while (newTotal < 0)
        {
            newTotal += MinutesInDay;
            DecrementDay();
        }

        CurrentTime = TimeSpan.FromMinutes(newTotal);
        TimeChanged?.Invoke(CurrentTime);
    }

    private void IncrementDay()
    {
        if (GameStateManager.Instance == null)
        {
            Debug.LogError("[WorldTime] GameStateManager.Instance is missing.");
            return;
        }

        int newDay = GameStateManager.Instance.CurrentDay + 1;
        GameStateManager.Instance.SetDay(newDay);
        DayChanged?.Invoke(newDay);
    }

    private void DecrementDay()
    {
        if (GameStateManager.Instance == null)
        {
            Debug.LogError("[WorldTime] GameStateManager.Instance is missing.");
            return;
        }

        int newDay = Mathf.Max(1, GameStateManager.Instance.CurrentDay - 1);
        GameStateManager.Instance.SetDay(newDay);
        DayChanged?.Invoke(newDay);
    }
}