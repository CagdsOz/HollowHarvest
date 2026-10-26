using UnityEngine;

namespace HollowHarvest
{
    public enum CareResult { Fed, Petted, Milked, FeedCooldown, PetCooldown, AlreadyMilked, FullyFed, NoPetNeeded, Unavailable }

    [RequireComponent(typeof(AnimalStats))]
    public sealed class CowCare : MonoBehaviour
    {
        [SerializeField] private MilkCounter milkCounter;
        [SerializeField, Min(0)] private float feedSatietyGain = 25;
        [Tooltip("In-game minutes, using the existing Game Clock.")]
        [SerializeField, Min(0)] private float feedCooldownMinutes = 30;
        [SerializeField, Min(0)] private float petStressReduction = 15;
        [SerializeField, Min(0)] private float petHealthGain = 2;
        [SerializeField, Min(0)] private float petCooldownMinutes = 10;
        private AnimalStats stats;
        private readonly CowCareSchedule schedule = new CowCareSchedule();
        private GameClock Clock => stats != null ? stats.Clock : null;
        public double FeedRemaining => Clock != null ? schedule.FeedRemaining(Clock.TotalMinutes) : 0;
        public double PetRemaining => Clock != null ? schedule.PetRemaining(Clock.TotalMinutes) : 0;
        public bool MilkedToday => Clock != null && !schedule.CanMilk(Clock.Day);

        private void Awake() => stats = GetComponent<AnimalStats>();

        public CareResult Feed()
        {
            if (Clock == null) return CareResult.Unavailable;
            stats.Synchronize();
            if (FeedRemaining > 0) return CareResult.FeedCooldown;
            if (feedSatietyGain <= 0) return CareResult.Unavailable;
            if (stats.Hunger >= 100) return CareResult.FullyFed;
            if (!schedule.TryFeed(Clock.TotalMinutes, feedCooldownMinutes)) return CareResult.FeedCooldown;
            stats.SetStatus(stats.Health, stats.Hunger + feedSatietyGain, stats.Stress);
            return CareResult.Fed;
        }
        public CareResult Pet()
        {
            if (Clock == null) return CareResult.Unavailable;
            stats.Synchronize();
            if (PetRemaining > 0) return CareResult.PetCooldown;
            if ((stats.Stress <= 0 || petStressReduction <= 0) && (stats.Health >= 100 || petHealthGain <= 0))
                return CareResult.NoPetNeeded;
            if (!schedule.TryPet(Clock.TotalMinutes, petCooldownMinutes)) return CareResult.PetCooldown;
            stats.SetStatus(stats.Health + petHealthGain, stats.Hunger, stats.Stress - petStressReduction);
            return CareResult.Petted;
        }
        public CareResult Milk()
        {
            if (Clock == null || milkCounter == null || !milkCounter.CanAddMilk) return CareResult.Unavailable;
            if (!schedule.TryMilk(Clock.Day)) return CareResult.AlreadyMilked;
            milkCounter.AddMilk();
            return CareResult.Milked;
        }
    }
}
