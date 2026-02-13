using System;
using System.Collections.Generic;
using UnityEngine;

namespace WorldTime
{
    public class TimeTriggerSystem : MonoBehaviour
    {
        [Serializable]
        public class Trigger
        {
            public string id;            // e.g. "Morning", "Night"
            [Range(0, 23)] public int hour;
            [Range(0, 59)] public int minute;

            // Optional: restrict to a specific day (0 = every day)
            public int onlyDay = 0;
        }

        [SerializeField] private WorldTime worldTime;
        [SerializeField] private List<Trigger> triggers = new List<Trigger>();

        // Fired when a trigger time is crossed
        public event Action<string> Triggered; // id

        private int _lastDay;
        private int _lastMinuteOfDay;
        private readonly HashSet<string> _firedToday = new HashSet<string>();

        private void Awake()
        {
            if (worldTime == null)
                worldTime = FindFirstObjectByType<WorldTime>();

            // Initialize last known time
            _lastDay = worldTime.CurrentDay;
            _lastMinuteOfDay = MinuteOfDay(worldTime.CurrentTime);
        }

        private void OnEnable()
        {
            if (worldTime == null) return;

            worldTime.WorldTimeChanged += OnTimeChanged;
            worldTime.WorldDayChanged += OnDayChanged;
        }

        private void OnDisable()
        {
            if (worldTime == null) return;

            worldTime.WorldTimeChanged -= OnTimeChanged;
            worldTime.WorldDayChanged -= OnDayChanged;
        }

        private void OnDayChanged(int newDay)
        {
            _firedToday.Clear();
        }

        private void OnTimeChanged(object sender, TimeSpan newTime)
        {
            int newDay = worldTime.CurrentDay;
            int newMinute = MinuteOfDay(newTime);

            // If day jumped (crossed midnight), handle:
            if (newDay != _lastDay)
            {
                // 1) Check remaining triggers in old day: (_lastMinute..1439)
                FireCrossing(_lastDay, _lastMinuteOfDay, 1439);

                // 2) New day started, clear fired set already via event, but safe:
                _firedToday.Clear();

                // 3) Check triggers from 0..newMinute in new day
                FireCrossing(newDay, 0, newMinute);
            }
            else
            {
                // Same day: check crossing range
                FireCrossing(newDay, _lastMinuteOfDay, newMinute);
            }

            _lastDay = newDay;
            _lastMinuteOfDay = newMinute;
        }

        private void FireCrossing(int day, int fromMinute, int toMinute)
        {
            if (toMinute <= fromMinute) return;

            foreach (var t in triggers)
            {
                if (t == null) continue;

                if (t.onlyDay != 0 && t.onlyDay != day)
                    continue;

                int triggerMinute = t.hour * 60 + t.minute;

                // fire if crossed: (from, to]  (e.g. from 600 to 615 triggers 610)
                if (triggerMinute > fromMinute && triggerMinute <= toMinute)
                {
                    if (_firedToday.Add(t.id))
                    {
                        Debug.Log($"[TimeTrigger] Day {day} crossed {t.hour:00}:{t.minute:00} => {t.id}");
                        Triggered?.Invoke(t.id);
                    }
                }
            }
        }

        private static int MinuteOfDay(TimeSpan time)
        {
            int m = (int)time.TotalMinutes;
            if (m < 0) m = 0;
            if (m > 1439) m = 1439;
            return m;
        }
    }
}
