using System;
using UnityEngine;

namespace WorldTime
{
    public class WorldTime : MonoBehaviour
    {
        // Keep this so other scripts referencing CurrentTime won't break
        public TimeSpan CurrentTime { get; private set; }

        // Keep your event so existing subscribers still work
        public event EventHandler<TimeSpan> WorldTimeChanged;

        [Header("Start Time")]
        [SerializeField] private int startHour = 6;
        [SerializeField] private int startMinute = 0;

        private void Awake()
        {
            // Set initial time to 06:00
            CurrentTime = new TimeSpan(startHour, startMinute, 0);

            // Optional: notify listeners immediately so UI/light updates at start
            WorldTimeChanged?.Invoke(this, CurrentTime);
        }

        // IMPORTANT: No Update() time ticking anymore.
        // If you currently have Update() adding time, remove/disable it.

        public void AdvanceMinutes(int minutes)
        {
            if (minutes <= 0) return;

            CurrentTime = CurrentTime.Add(TimeSpan.FromMinutes(minutes));

            // If you want to wrap after 24h:
            if (CurrentTime.TotalMinutes >= 1440)
                CurrentTime = TimeSpan.FromMinutes(CurrentTime.TotalMinutes % 1440);

            WorldTimeChanged?.Invoke(this, CurrentTime);
        }
    }
}
