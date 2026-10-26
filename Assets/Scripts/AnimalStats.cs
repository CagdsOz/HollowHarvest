using UnityEngine;

namespace HollowHarvest
{
    public sealed class AnimalStats : MonoBehaviour
    {
        [SerializeField] private GameClock clock;
        [SerializeField] private string animalName = "Clover";
        [SerializeField, Range(0, 100)] private float health = 100;
        [SerializeField, Range(0, 100)] private float hunger = 100;
        [SerializeField, Range(0, 100)] private float stress;
        [SerializeField, Min(0)] private float hungerLossPerGameHour = 2;
        [SerializeField, Range(0, 100)] private float hungryThreshold = 35;
        [SerializeField, Range(0, 100)] private float severelyHungryThreshold = 15;
        [SerializeField, Min(0)] private float stressGainPerHungryHour = 6;
        [SerializeField, Min(0)] private float healthLossPerSeverelyHungryHour = 3;
        [SerializeField, Range(0, 100)] private float nervousStressThreshold = 60;
        [SerializeField, Range(0, 100)] private float weakHealthThreshold = 30;
        private double lastTime;
        private bool initialized;

        public string AnimalName => animalName;
        public float Health => health;
        public float Hunger => hunger;
        public float Stress => stress;
        public GameClock Clock => clock;
        public AnimalBehavior Behavior => AnimalConditionRules.Evaluate(health, hunger, stress,
            weakHealthThreshold, hungryThreshold, nervousStressThreshold);

        private void Start()
        {
            if (clock != null) InitializeTime();
            else Debug.LogError("AnimalStats requires the farm Game Clock.", this);
        }

        private void Update() => Synchronize();

        private void InitializeTime()
        {
            if (initialized) return;
            lastTime = clock.TotalMinutes;
            initialized = true;
        }

        // Care actions synchronize first, so earlier elapsed time is never applied to newly fed stats.
        public void Synchronize()
        {
            if (clock == null) return;
            InitializeTime();
            double now = clock.TotalMinutes;
            double minutes = System.Math.Max(0, now - lastTime);
            lastTime = now;
            AdvanceStats(ref health, ref hunger, ref stress, minutes, hungerLossPerGameHour,
                hungryThreshold, severelyHungryThreshold, stressGainPerHungryHour, healthLossPerSeverelyHungryHour);
        }

        public static void AdvanceStats(ref float health, ref float satiety, ref float stress, double minutes,
            float satietyLossPerHour, float hungryAt, float severelyHungryAt,
            float stressGainPerHour, float healthLossPerHour)
        {
            // Integrate only the time spent under each threshold. Works even across a multi-day jump.
            double hungryMinutes = MinutesBelowThreshold(satiety, satietyLossPerHour, hungryAt, minutes);
            double severeMinutes = MinutesBelowThreshold(satiety, satietyLossPerHour, severelyHungryAt, minutes);
            stress = Mathf.Clamp(stress + (float)(hungryMinutes / 60d) * Mathf.Max(0, stressGainPerHour), 0, 100);
            health = Mathf.Clamp(health - (float)(severeMinutes / 60d) * Mathf.Max(0, healthLossPerHour), 0, 100);
            satiety = HungerAfterElapsedTime(satiety, satietyLossPerHour, minutes);
        }

        private static double MinutesBelowThreshold(float satiety, float lossPerHour, float threshold, double minutes)
        {
            minutes = System.Math.Max(0, minutes);
            if (satiety <= threshold) return minutes;
            if (lossPerHour <= 0) return 0;
            double minutesUntilThreshold = (satiety - threshold) / (double)lossPerHour * 60d;
            return System.Math.Max(0, minutes - minutesUntilThreshold);
        }

        public static float HungerAfterElapsedTime(float current, float hourlyLoss, double minutes)
            => Mathf.Clamp(current - (float)(System.Math.Max(0, minutes) / 60d)
                * Mathf.Max(0, hourlyLoss), 0, 100);

        public void SetStatus(float newHealth, float newHunger, float newStress)
        {
            health = Mathf.Clamp(newHealth, 0, 100);
            hunger = Mathf.Clamp(newHunger, 0, 100);
            stress = Mathf.Clamp(newStress, 0, 100);
        }

        private void OnValidate()
        {
            SetStatus(health, hunger, stress);
            hungerLossPerGameHour = Mathf.Max(0, hungerLossPerGameHour);
            severelyHungryThreshold = Mathf.Min(severelyHungryThreshold, hungryThreshold);
            stressGainPerHungryHour = Mathf.Max(0, stressGainPerHungryHour);
            healthLossPerSeverelyHungryHour = Mathf.Max(0, healthLossPerSeverelyHungryHour);
        }
    }
}
