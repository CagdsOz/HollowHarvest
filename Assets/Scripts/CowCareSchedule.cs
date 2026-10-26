using System;

namespace HollowHarvest
{
    // Per-cow runtime state, independent of the UI. Closing the panel cannot reset it.
    public sealed class CowCareSchedule
    {
        private double nextFeed = double.NegativeInfinity;
        private double nextPet = double.NegativeInfinity;
        public int LastMilkedDay { get; private set; } = -1;

        public double FeedRemaining(double now) => Math.Max(0, nextFeed - now);
        public double PetRemaining(double now) => Math.Max(0, nextPet - now);
        public bool TryFeed(double now, double cooldown)
        {
            if (FeedRemaining(now) > 0) return false;
            nextFeed = now + Math.Max(0, cooldown);
            return true;
        }
        public bool TryPet(double now, double cooldown)
        {
            if (PetRemaining(now) > 0) return false;
            nextPet = now + Math.Max(0, cooldown);
            return true;
        }
        public bool CanMilk(int day) => day >= 1 && day > LastMilkedDay;
        public bool TryMilk(int day)
        {
            if (!CanMilk(day)) return false;
            LastMilkedDay = day;
            return true;
        }
    }
}
