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
}
