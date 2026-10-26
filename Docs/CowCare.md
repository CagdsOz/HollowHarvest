# Sprint 3: cow care

## Set up the working farm

1. Exit Play mode and let Unity finish compiling. Use **Assets > Refresh** if the new menu has not appeared.
2. Choose **HollowHarvest > Upgrade Cow Care (Sprint 3)**. Save any edits when prompted. The command opens `Assets/Scenes/FarmPrototype.unity`, upgrades the existing cow panel, adds care components to its cows, connects a shared milk counter, and saves the scene.
3. Press **Play**, approach Clover, and press **E**. The panel now has **Feed**, **Pet**, **Milk**, and the original **Close** button. Escape still closes it.

Run the upgrade once. Running it again reconnects references and reapplies the panel layout without duplicating components/buttons or resetting cow balance settings. Its scene changes are one Undo operation. It requires the working Sprint 2 cow panel; it does not rebuild the farm or change doors, interiors, input bindings, movement, camera, or clock.

The **MILK** HUD appears in the top-right corner. The `MilkCounter` component lives on the existing **Game Clock** object so it remains active both outdoors and indoors.

## What each component does

- **AnimalStats:** owns name, health, satiety (the existing Hunger field), stress, and time-based deterioration. All stats stay between 0 and 100.
- **AnimalConditionRules:** derives the displayed condition from stats. Priority is **Weak > Nervous > Hungry > Normal**. This changes the condition label; wandering remains the existing normal cow movement. No special AI or supernatural behavior is added.
- **CowCare:** applies feeding/petting and awards milk. Its `CowCareSchedule` holds independent cooldowns and the last milking day for that cow.
- **CowStatusPanel:** displays values and invokes the selected cow's actions. It owns no stats, milk allowance, or cooldown state.
- **MilkCounter / MilkCounterHud:** hold and display the shared session milk total without an inventory system.

To add another cow, duplicate your upgraded cow and give it another name and valid grazing marker/area. Its stats and care schedule are independent; its `Milk Counter` reference should point to the same counter on Game Clock. Both cows can provide one milk on the same day.

## Default balancing values

Select **Cow - Clover** in the Hierarchy. All these values are editable in its components:

| Component | Setting | Default |
| --- | --- | --- |
| Animal Stats | Hunger Loss Per Game Hour | 2 satiety points |
| Animal Stats | Hungry Threshold | 35 or lower |
| Animal Stats | Severely Hungry Threshold | 15 or lower |
| Animal Stats | Stress Gain Per Hungry Hour | 6 points |
| Animal Stats | Health Loss Per Severely Hungry Hour | 3 points |
| Animal Stats | Nervous Stress Threshold | 60 or higher |
| Animal Stats | Weak Health Threshold | 30 or lower |
| Cow Care | Feed Satiety Gain | 25 points |
| Cow Care | Feed Cooldown Minutes | 30 in-game minutes |
| Cow Care | Pet Stress Reduction | 15 points |
| Cow Care | Pet Health Gain | 2 points, capped at 100 |
| Cow Care | Pet Cooldown Minutes | 10 in-game minutes |

Feeding stops further hunger-related stress/damage once satiety is above the corresponding threshold. It does not instantly remove accumulated stress or restore health; petting helps those stats. A fully fed cow does not consume a feed cooldown. Petting a cow already at full health and zero stress does not consume a pet cooldown.

The panel shows remaining cooldowns in **game minutes**. Buttons remain clickable so early attempts can explain the cooldown or daily limit. A successful milk always adds **1**; there are no health/satiety restrictions on milking in this sprint. The allowance becomes available again at **midnight**, even if the panel stays open. Missed days do not accumulate milk.

All stat changes, cooldowns, and daily allowances use the existing game clock. With Minutes Per Second at 0, those timers stop; immediate care actions still work if available. At the Day 30 clock cap, no further time-based reset occurs. Only the short UI feedback timeout uses real time so messages remain readable when testing fast or paused time.

Cow stats, cooldowns, milking history, and milk total survive entering/exiting buildings because the same scene objects stay alive. This is session state: stopping Play resets it. There is no save-to-disk system, death system, selling, or inventory.

## Practical test sequence

Use Play-mode Inspector changes for testing; they revert when you stop Play. Inspector names use **Hunger** to mean satiety (100 = fully fed).

1. **Feed:** enter Play, set Game Clock's Minutes Per Second to **0**, and set the cow's Hunger to **50**. Open the panel and click Feed: expect **75** and a feedback message. Click again: expect a cooldown message with no additional gain. Close/reopen the panel: the cooldown remains. Set time speed to **10**; after about three real seconds the 30-minute cooldown has elapsed. Feed again and confirm the value never exceeds 100.
2. **Pet:** pause the clock again and set Health to **80**, Stress to **40**. Click Pet: expect Health **82**, Stress **25**. Repeated clicks during the cooldown must not change them. At speed **10**, the 10-minute cooldown takes about one real second. Also test Health **99** and Stress **5**: petting clamps them to **100** and **0**.
3. **Milk:** click Milk and confirm the top-right counter increases by **1**. Click again: expect “Already milked today” and no counter increase. Close/reopen the panel and try again; the daily limit remains.
4. **Daily reset:** stop Play. Set the clock's Starting Day to **1**, Starting Hour to **23**, Starting Minute to **59**, and Minutes Per Second to **0**. Enter Play and milk the cow. Set time speed to **60**; in about one real second the clock reaches Day 2. Milk again: exactly one additional milk is allowed. Test with the panel left open across midnight.
5. **Stat changes:** in Play, pause the clock, set Health **80**, Hunger **10**, and Stress **20**, then set speed to **60**. Each real second is one game hour: expect roughly -2 satiety, +6 stress, and -3 health per second. Pause again and verify all three stop changing. Leave accelerated time running long enough to confirm the 0–100 clamps.
6. **Behavior:** pause time. Use Health/Hunger/Stress values **100/100/0** for Normal, **100/30/0** for Hungry, **100/30/70** for Nervous, and **20/30/70** for Weak. Confirm priority and immediate panel updates. Restore healthier stats or apply care and confirm the label recovers accordingly.
7. **Buildings:** feed/pet/milk, note the values and total, close the panel, then enter and exit both buildings. With the clock paused, state should be unchanged. With time running, stats/cooldowns should progress. Milking again on the same day must still fail.
8. **Regression:** check E, Escape, Close, player movement locking, wandering, F transitions, day/night lighting, and the day/time HUD. Repeat the basic care flow in a standalone build. Restore clock starting values to **1 / 06:00**, speed **1**, and save after leaving Play mode.

## Verification and changed files

`powershell -NoProfile -ExecutionPolicy Bypass -File Tools/ValidateFarmScripts.ps1` compiles runtime and Editor scripts separately against the installed Unity assemblies. The checks now cover cooldown boundaries, independent cows, daily reset/duplicate milking, threshold-crossing stat decay, accelerated versus incremental time, stat clamps, and behavior priority, along with the existing hunger and grazing tests. These managed checks do not simulate UI clicks or building transitions; use the checklist above in Play mode.

Modified: `Assets/Scripts/AnimalStats.cs`, `Assets/Scripts/CowStatusPanel.cs`, `Tools/Tests/FarmLogicChecks.cs`, and `README.md`.

Added: `Assets/Scripts/AnimalConditionRules.cs`, `CowCare.cs`, `CowCareSchedule.cs`, `MilkCounter.cs`, `MilkCounterHud.cs`; `Assets/Editor/CowCareSetup.cs`; this guide; and Unity script metadata. Running the menu upgrades the existing scene without creating another scene or new graphic assets.
