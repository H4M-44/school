using System;
using UnityEngine;

namespace WorldTime
{
    public class WorldLight3D : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private WorldTime _worldTime;
        [SerializeField] private Light _sunLight; // Directional Light

        [Header("Sun settings")]
        [SerializeField] private Gradient _sunColorOverDay;
        [SerializeField] private AnimationCurve _sunIntensityOverDay = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] private float _maxIntensity = 1.2f;

        [Header("Sun rotation")]
        [Tooltip("X rotation at midnight (0:00).")]
        [SerializeField] private float _midnightAngle = -90f;
        [Tooltip("X rotation at noon (12:00).")]
        [SerializeField] private float _noonAngle = 90f;

        private void Awake()
        {
            if (_worldTime == null)
                _worldTime = FindFirstObjectByType<WorldTime>();

            if (_sunLight == null)
                _sunLight = FindFirstObjectByType<Light>();

            if (_worldTime == null || _sunLight == null)
            {
                Debug.LogError("WorldLight3D: Missing WorldTime or Sun Light reference.", this);
                enabled = false;
                return;
            }

            _worldTime.WorldTimeChanged += OnWorldTimeChanged;

            // init once
            OnWorldTimeChanged(this, _worldTime.CurrentTime);
        }

        private void OnDestroy()
        {
            if (_worldTime != null)
                _worldTime.WorldTimeChanged -= OnWorldTimeChanged;
        }

        private void OnWorldTimeChanged(object sender, TimeSpan newTime)
        {
            float t = PercentOfDay(newTime);

            // 1) Rotate sun (X axis)
            float sunAngleX = Mathf.Lerp(_midnightAngle, _noonAngle, t * 2f);
            if (t > 0.5f) sunAngleX = Mathf.Lerp(_noonAngle, _midnightAngle + 360f, (t - 0.5f) * 2f);
            transform.rotation = Quaternion.Euler(sunAngleX, 170f, 0f);

            // 2) Color
            if (_sunColorOverDay != null)
                _sunLight.color = _sunColorOverDay.Evaluate(t);

            // 3) Intensity
            float intensity01 = _sunIntensityOverDay.Evaluate(t);
            _sunLight.intensity = intensity01 * _maxIntensity;
        }

        private float PercentOfDay(TimeSpan timeSpan)
        {
            float minutes = (float)(timeSpan.TotalMinutes % WorldTimeConstants.MinutesInDay);
            return minutes / WorldTimeConstants.MinutesInDay; // 0..1
        }
    }
}
