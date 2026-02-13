using System;
using TMPro;
using UnityEngine;

namespace WorldTime
{
    [RequireComponent(typeof(TMP_Text))]
    public class WorldTimeDisplay : MonoBehaviour
    {
        [SerializeField] private WorldTime _worldTime;

        private TMP_Text _text;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();

            if (_worldTime == null)
            {
                Debug.LogError("WorldTimeDisplay: _worldTime is not assigned.", this);
                enabled = false;
                return;
            }

            _worldTime.WorldTimeChanged += OnWorldTimeChanged;
            _worldTime.WorldDayChanged += OnWorldDayChanged; // NEW

            RefreshText();
        }

        private void OnDestroy()
        {
            if (_worldTime == null) return;

            _worldTime.WorldTimeChanged -= OnWorldTimeChanged;
            _worldTime.WorldDayChanged -= OnWorldDayChanged; // NEW
        }

        private void OnWorldTimeChanged(object sender, TimeSpan newTime)
        {
            RefreshText();
        }

        private void OnWorldDayChanged(int newDay)
        {
            RefreshText();
        }

        private void RefreshText()
        {
            // Example: "Day 3  06:15"
            _text.SetText($"Day {_worldTime.CurrentDay}  {_worldTime.CurrentTime:hh\\:mm}");
        }
    }
}
