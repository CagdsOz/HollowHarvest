using UnityEngine;

namespace HollowHarvest
{
    // Descriptive state only. Future behavior controllers can change it without owning UI.
    public enum AnimalBehavior { Normal, Nervous, Aggressive }

    public sealed class AnimalStats : MonoBehaviour
    {
        [SerializeField] private GameClock clock;
        [SerializeField] private string animalName = "Clover";
        [SerializeField, Range(0, 100)] private float health = 100;
        [SerializeField, Range(0, 100)] private float hunger = 100;
        [SerializeField, Range(0, 100)] private float stress;
        [SerializeField] private AnimalBehavior behavior = AnimalBehavior.Normal;
        [SerializeField, Min(0)] private float hungerLossPerGameHour = 2;
        private double lastTime;

        public string AnimalName => animalName;
        public float Health => health;
        public float Hunger => hunger;
        public float Stress => stress;
        public AnimalBehavior Behavior => behavior;

        private void Start()
        {
            if (clock != null) lastTime = clock.TotalMinutes;
            else Debug.LogError("AnimalStats requires the farm Game Clock.", this);
        }

        private void Update()
        {
            if (clock == null) return;
            double now = clock.TotalMinutes;
            double minutes = System.Math.Max(0, now - lastTime);
            lastTime = now;
            hunger = HungerAfterElapsedTime(hunger, hungerLossPerGameHour, minutes);
        }

        public static float HungerAfterElapsedTime(float current, float hourlyLoss, double minutes)
            => Mathf.Clamp(current - (float)(System.Math.Max(0, minutes) / 60d)
                * Mathf.Max(0, hourlyLoss), 0, 100);

        public void SetStatus(float newHealth, float newHunger, float newStress, AnimalBehavior newBehavior)
        {
            health = Mathf.Clamp(newHealth, 0, 100);
            hunger = Mathf.Clamp(newHunger, 0, 100);
            stress = Mathf.Clamp(newStress, 0, 100);
            behavior = newBehavior;
        }

        private void OnValidate()
        {
            SetStatus(health, hunger, stress, behavior);
            hungerLossPerGameHour = Mathf.Max(0, hungerLossPerGameHour);
        }
    }
}
