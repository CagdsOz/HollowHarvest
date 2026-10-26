namespace HollowHarvest
{
    // Preserve earlier enum values. Aggressive is reserved, never selected by normal care rules.
    public enum AnimalBehavior { Normal = 0, Nervous = 1, Aggressive = 2, Hungry = 3, Weak = 4 }

    public static class AnimalConditionRules
    {
        public static AnimalBehavior Evaluate(float health, float satiety, float stress,
            float weakAt, float hungryAt, float nervousAt)
        {
            if (health <= weakAt) return AnimalBehavior.Weak;
            if (stress >= nervousAt) return AnimalBehavior.Nervous;
            if (satiety <= hungryAt) return AnimalBehavior.Hungry;
            return AnimalBehavior.Normal;
        }
    }
}
