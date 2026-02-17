using System;
using UnityEngine;
using System.Linq;


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
        
        public void SetTime(int day, TimeSpan time)
        {
            day = Mathf.Max(1, day);

            bool dayChanged = day != CurrentDay;
            CurrentDay = day;
            CurrentTime = time;

            if (dayChanged)
                WorldDayChanged?.Invoke(CurrentDay);

            WorldTimeChanged?.Invoke(this, CurrentTime);
        }

        // Convenience: "HH:mm"
        public void SetTimeHHmm(int day, string hhmm)
        {
            if (!TryParseHHmm(hhmm, out var time))
            {
                Debug.LogError($"Invalid time format: {hhmm}");
                return;
            }
            SetTime(day, time);
        }

        private bool TryParseHHmm(string s, out TimeSpan time)
        {
            time = default;
            if (string.IsNullOrWhiteSpace(s)) return false;

            s = s.Trim();

            // If it contains a date, try to extract the time part (e.g. "31/12/1899 15:15")
            if (s.Contains(" "))
            {
                var last = s.Split(' ').Last();
                // last might be "15:15" or "15:15:00"
                s = last;
            }

            // handle "HH:mm:ss"
            if (s.Count(ch => ch == ':') == 2)
            {
                var p = s.Split(':');
                s = $"{p[0]}:{p[1]}";
            }

            var parts = s.Split(':');
            if (parts.Length < 2) return false;

            if (!int.TryParse(parts[0], out var h)) return false;
            if (!int.TryParse(parts[1], out var m)) return false;

            if (h < 0 || h > 23 || m < 0 || m > 59) return false;

            time = new TimeSpan(h, m, 0);
            return true;
        }







    }
}
