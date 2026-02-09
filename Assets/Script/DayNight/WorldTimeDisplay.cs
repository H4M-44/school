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

            // Optional: show initial time immediately
            _text.SetText(_worldTime.CurrentTime.ToString(@"hh\:mm"));
        }

        private void OnDestroy()
        {
            if (_worldTime != null)
                _worldTime.WorldTimeChanged -= OnWorldTimeChanged;
        }

        private void OnWorldTimeChanged(object sender, TimeSpan newTime)
        {
            _text.SetText(newTime.ToString(@"hh\:mm"));
        }
    }
}
