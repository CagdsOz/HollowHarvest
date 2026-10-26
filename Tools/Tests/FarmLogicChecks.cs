using System;
using HollowHarvest;
using UnityEngine;
internal static class LogicChecks
{
    static void Equal(float actual, float expected, string name)
    {
        if (Math.Abs(actual - expected) > 0.0001f) throw new Exception(name + ": " + actual + " != " + expected);
    }
    static void Main()
    {
        CareChecks();
        Equal(AnimalStats.HungerAfterElapsedTime(100, 2, 60), 98, "one game hour");
        Equal(AnimalStats.HungerAfterElapsedTime(100, 2, 1440), 52, "full game day");
        Equal(AnimalStats.HungerAfterElapsedTime(2, 2, 300), 0, "hunger lower bound");
        Equal(AnimalStats.HungerAfterElapsedTime(80, 2, 0), 80, "paused clock");
        Equal(AnimalStats.HungerAfterElapsedTime(80, 2, -60), 80, "clock rollback");
        Equal(AnimalStats.HungerAfterElapsedTime(80, -2, 60), 80, "negative rate");
        Equal(AnimalStats.HungerAfterElapsedTime(150, 2, 0), 100, "upper bound");
        var random = new System.Random(42);
        var center = new Vector2(10, -7);
        var half = new Vector2(4, 2.75f);
        for (int i = 0; i < 10000; i++)
        {
            var point = new Vector2((float)random.NextDouble() * 200 - 100, (float)random.NextDouble() * 200 - 100);
            var result = CowWander.ClampPosition(point, center, half);
            if (result.x < 6 || result.x > 14 || result.y < -9.75f || result.y > -4.25f)
                throw new Exception("Pasture clamp escaped its bounds.");
            var again = CowWander.ClampPosition(result, center, half);
            Equal(again.x, result.x, "clamp idempotent x");
            Equal(again.y, result.y, "clamp idempotent y");
        }
        Console.WriteLine("PASS: 7 hunger edge cases and 10,000 pasture clamp/idempotence cases.");
    }

    static void Require(bool condition, string name)
    {
        if (!condition) throw new Exception(name);
    }

    static void CareChecks()
    {
        var cow = new CowCareSchedule();
        Require(cow.TryFeed(360, 30), "first feed available at 06:00");
        Require(!cow.TryFeed(360, 30), "same-frame double feed blocked");
        Require(!cow.TryFeed(389.99, 30), "feed blocked before cooldown boundary");
        Equal((float)cow.FeedRemaining(375), 15, "remaining in-game minutes");
        Require(cow.TryFeed(390, 30), "feed allowed exactly at boundary");
        Require(cow.TryPet(390, 10), "pet independent of feed cooldown");
        Require(!cow.TryPet(399.99, 10), "pet cooldown enforced");
        Require(cow.TryPet(400, 10), "pet available at boundary");
        Require(cow.TryMilk(1), "first day's milk");
        Require(!cow.TryMilk(1), "same day's second milk rejected");
        Require(cow.TryMilk(2), "new day unlocks milk");
        Require(cow.TryMilk(5), "skipped days unlock a single milk");
        Require(!cow.TryMilk(5) && !cow.TryMilk(4), "duplicate day and clock rollback rejected");
        var otherCow = new CowCareSchedule();
        Require(otherCow.TryMilk(5), "other cow has its own daily allowance");
        Require(otherCow.TryFeed(400, 30), "other cow has its own cooldown");

        // From 40 satiety at 10/hour, 3h crosses 35 after 0.5h and 15 after 2.5h.
        float health = 100, satiety = 40, stress = 0;
        AnimalStats.AdvanceStats(ref health, ref satiety, ref stress, 180, 10, 35, 15, 6, 3);
        Equal(satiety, 10, "three hours of hunger loss");
        Equal(stress, 15, "only 2.5 hours of hungry stress");
        Equal(health, 98.5f, "only 0.5 hours of severe hunger damage");
        float smallHealth = 100, smallSatiety = 40, smallStress = 0;
        for (int i = 0; i < 180; i++)
            AnimalStats.AdvanceStats(ref smallHealth, ref smallSatiety, ref smallStress, 1, 10, 35, 15, 6, 3);
        Require(Math.Abs(smallHealth - health) < 0.002 && Math.Abs(smallStress - stress) < 0.002
            && Math.Abs(smallSatiety - satiety) < 0.002, "large and incremental time advances agree");
        AnimalStats.AdvanceStats(ref health, ref satiety, ref stress, 0, 10, 35, 15, 6, 3);
        Equal(stress, 15, "paused clock stops stress");
        Equal(health, 98.5f, "paused clock stops health loss");
        AnimalStats.AdvanceStats(ref health, ref satiety, ref stress, 100000, 10, 35, 15, 6, 3);
        Equal(health, 0, "health lower clamp");
        Equal(satiety, 0, "satiety lower clamp");
        Equal(stress, 100, "stress upper clamp");
        health = 80; satiety = 10; stress = 20;
        AnimalStats.AdvanceStats(ref health, ref satiety, ref stress, 60, 0, 35, 15, 6, 3);
        Equal(health, 77, "already severe hunger with zero satiety decay");
        Equal(stress, 26, "already hungry with zero satiety decay");
        health = 80; satiety = 80; stress = 20;
        AnimalStats.AdvanceStats(ref health, ref satiety, ref stress, 60, 0, 35, 15, 6, 3);
        Equal(health, 80, "fed cow takes no hunger damage");
        Equal(stress, 20, "fed cow gains no hunger stress");

        Require(AnimalConditionRules.Evaluate(100, 100, 0, 30, 35, 60) == AnimalBehavior.Normal, "normal condition");
        Require(AnimalConditionRules.Evaluate(100, 35, 0, 30, 35, 60) == AnimalBehavior.Hungry, "hungry boundary");
        Require(AnimalConditionRules.Evaluate(100, 10, 60, 30, 35, 60) == AnimalBehavior.Nervous, "nervous priority");
        Require(AnimalConditionRules.Evaluate(30, 10, 90, 30, 35, 60) == AnimalBehavior.Weak, "weak priority");
        Console.WriteLine("PASS: care cooldowns, per-cow daily milking, threshold integration, clamping, and behavior rules.");
    }
}
