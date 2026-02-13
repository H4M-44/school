using System;
using UnityEngine;

namespace WorldTime
{
    public class WorldTime : MonoBehaviour
    {
        public TimeSpan CurrentTime { get; private set; }

        // NEW: day counter (Day 1 = start day)
        public int CurrentDay { get; private set; } = 1;

        public event EventHandler<TimeSpan> WorldTimeChanged;

        // NEW: day change event (optional but useful)
        public event Action<int> WorldDayChanged; // newDay

        [Header("Start Time")]
        [SerializeField] private int startHour = 6;
        [SerializeField] private int startMinute = 0;

        [Header("Start Day")]
        [SerializeField] private int startDay = 1;

        private void Awake()
        {
            CurrentDay = Mathf.Max(1, startDay);
            CurrentTime = new TimeSpan(startHour, startMinute, 0);

            WorldDayChanged?.Invoke(CurrentDay);
            WorldTimeChanged?.Invoke(this, CurrentTime);
        }

        public void AdvanceMinutes(int minutes)
        {
            if (minutes <= 0) return;

            int oldDay = CurrentDay;

            double totalMinutes = CurrentTime.TotalMinutes + minutes;

            if (totalMinutes >= 1440)
            {
                int dayAdd = (int)(totalMinutes / 1440);
                CurrentDay += dayAdd;
                totalMinutes = totalMinutes % 1440;
            }

            CurrentTime = TimeSpan.FromMinutes(totalMinutes);

            if (CurrentDay != oldDay)
                WorldDayChanged?.Invoke(CurrentDay);

            WorldTimeChanged?.Invoke(this, CurrentTime);
        }
    }
}
