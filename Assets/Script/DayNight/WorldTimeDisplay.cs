using UnityEngine;
using TMPro;
using System;

public class WorldTimeDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text timeText;

    private WorldTime _worldTime;
    private GameStateManager _gameState;
    private bool _isBound;

    private void OnEnable()
    {
        TryBind();
        RefreshAll();
    }

    private void Start()
    {
        if (!_isBound)
        {
            TryBind();
            RefreshAll();
        }
    }

    private void OnDisable()
    {
        Unbind();
    }

    public void TryBind()
    {
        Unbind();

        _worldTime = WorldTime.Instance;
        _gameState = GameStateManager.Instance;

        Debug.Log($"[WorldTimeDisplay] Bind result - WorldTime: {_worldTime != null}, GameState: {_gameState != null}");

        if (_worldTime != null)
        {
            _worldTime.TimeChanged += HandleTimeChanged;
            _worldTime.DayChanged += HandleDayChanged;
        }

        if (_gameState != null)
        {
            _gameState.DayChanged += HandleDayChanged;
        }

        _isBound = (_worldTime != null && _gameState != null);
    }

    private void Unbind()
    {
        if (_worldTime != null)
        {
            _worldTime.TimeChanged -= HandleTimeChanged;
            _worldTime.DayChanged -= HandleDayChanged;
        }

        if (_gameState != null)
        {
            _gameState.DayChanged -= HandleDayChanged;
        }

        _worldTime = null;
        _gameState = null;
        _isBound = false;
    }

    private void HandleTimeChanged(TimeSpan newTime)
    {
        RefreshTime(newTime);
    }

    private void HandleDayChanged(int newDay)
    {
        RefreshDay(newDay);
    }

    private void RefreshAll()
    {
        if (GameStateManager.Instance != null)
            RefreshDay(GameStateManager.Instance.CurrentDay);

        if (WorldTime.Instance != null)
            RefreshTime(WorldTime.Instance.CurrentTime);
    }

    private void RefreshDay(int day)
    {
        if (dayText != null)
            dayText.text = $"Day {day}";
    }

    private void RefreshTime(TimeSpan time)
    {
        if (timeText != null)
            timeText.text = time.ToString(@"hh\:mm");
    }
}