using System;
using UnityEngine;

namespace HollowHarvest
{
    public sealed class GameClock : MonoBehaviour
    {
        public const int LastDay = 30;
        public const int MinutesPerDay = 1440;
        [SerializeField, Range(1, LastDay)] private int startingDay = 1;
        [SerializeField, Range(0, 23)] private int startingHour = 6;
        [SerializeField, Range(0, 59)] private int startingMinute;
        [Tooltip("In-game minutes per real second. 1 = a 24-minute day; 0 pauses.")]
        [SerializeField, Min(0f)] private float minutesPerSecond = 1f;
        private double elapsedMinutes;
        private int lastPublishedMinute = -1;

        public int Day => (int)(elapsedMinutes / MinutesPerDay) + 1;
        public double TotalMinutes => elapsedMinutes;
        public int Hour => (int)(elapsedMinutes % MinutesPerDay) / 60;
        public int Minute => (int)(elapsedMinutes % 60);
        public float NormalizedTime => (float)(elapsedMinutes % MinutesPerDay / MinutesPerDay);
        public bool IsComplete => elapsedMinutes >= LastDay * MinutesPerDay - 1;
        public event Action TimeChanged;

        private void Awake()
        {
            elapsedMinutes = (Mathf.Clamp(startingDay, 1, LastDay) - 1) * MinutesPerDay
                + Mathf.Clamp(startingHour, 0, 23) * 60 + Mathf.Clamp(startingMinute, 0, 59);
        }

        private void Update() => Advance(Time.deltaTime);

        // Handles midnight and multiple skipped days, including at accelerated test speeds.
        public void Advance(float realSeconds)
        {
            if (float.IsNaN(realSeconds) || float.IsInfinity(realSeconds) || realSeconds < 0f) return;
            if (float.IsNaN(minutesPerSecond) || float.IsInfinity(minutesPerSecond)) return;
            elapsedMinutes = Math.Min(LastDay * MinutesPerDay - 1,
                elapsedMinutes + realSeconds * (double)Mathf.Max(0f, minutesPerSecond));
            int wholeMinute = (int)elapsedMinutes;
            if (wholeMinute == lastPublishedMinute) return;
            lastPublishedMinute = wholeMinute;
            TimeChanged?.Invoke();
        }
    }
}
