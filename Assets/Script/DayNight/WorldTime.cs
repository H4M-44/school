using System;
using System.Collections;
using UnityEngine;

namespace WorldTime
{
    public class WorldTime : MonoBehaviour
    {
        [SerializeField] private float _dayLength = 600f; // seconds per full day

        public event EventHandler<TimeSpan> WorldTimeChanged;

        public TimeSpan CurrentTime { get; private set; } = TimeSpan.Zero;

        private float MinuteLength => _dayLength / WorldTimeConstants.MinutesInDay;

        private Coroutine _routine;

        private void Start()
        {
            _routine = StartCoroutine(ClockRoutine());
        }

        private IEnumerator ClockRoutine()
        {
            while (true)
            {
                CurrentTime += TimeSpan.FromMinutes(1);
                WorldTimeChanged?.Invoke(this, CurrentTime);

                yield return new WaitForSeconds(MinuteLength);
            }
        }
    }
}
