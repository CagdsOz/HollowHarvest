# Cow interaction and enterable buildings

## Editor setup

1. Open the project in Unity 6000.5.5f1, exit Play mode, and allow imports/compilation to finish. Use **Assets > Refresh** if needed.
2. Choose **HollowHarvest > Add Cow and Building Interactions**. Save any scene edits when prompted. The command opens the existing `Assets/Scenes/FarmPrototype.unity`, adds the new objects, connects all references, and saves it.
3. Press **Play**. Clover is in the southeast pasture, initially at **(10, -7)**. Approach and press **E**. Click **Close** or press **Escape** to resume movement.
4. Approach the south face of the **farmhouse** or **barn**, then press **F**. Indoors, approach the south exit and press **F** to return.

The command preserves the Sprint 1 scene objects and their settings. Running it again does not duplicate objects or reset your new settings. Its scene changes form a single Undo operation. It requires the existing `PlayerMovement`, `CameraFollow`, `GameClock`, and the `Farmhouse`/`Barn` objects with their original box colliders. No new packages are needed.

Generated assets: `Assets/Art/Prototype/Cow.png` and six UI action reference assets under `Assets/Settings/FarmInteractionUI/` (`Point`, `Click`, `Navigate`, `Submit`, `Cancel`, `ScrollWheel`). The scene gains a **Cow and Building Systems** root plus **PlayerInteraction** on the existing Player. The menu command has to be run once; runtime scripts do not silently rebuild the scene.

## Inspector controls

| Object/component | Settings |
| --- | --- |
| Cow - Clover / Animal Stats | Animal Name, Health, Hunger, Stress, Behavior, Hunger Loss Per Game Hour, Game Clock |
| Cow - Clover / Cow Wander | Grazing Center, Grazing Size, Move Speed, Wandering Radius, Minimum/Maximum Idle Duration |
| Cow - Clover / Cow Interaction | Interaction Radius (default 2.5 units) |
| Farmhouse/Barn Door Zone / Door Interaction | Zone Size, Destination entrance marker, Destination Area |
| Interior / Exit Interaction Zone | Zone Size, Is Exit, outdoor Destination, Departing Area |
| Cow and Building Systems / Building Transition | Fade Duration (default 0.4 seconds per fade) |
| Game Clock | Minutes Per Second, as in Sprint 1 |

The grazing rectangle is world-aligned and centered on a **separate stationary marker**, not the cow. Select the cow to see its bounds and inspection radius. Keep the grazing area clear of buildings and at least as large as the cow sprite; movement insets its bounds by the cow's visible footprint. Cow physics is kinematic with a trigger collider, so the player cannot push it outside the pasture. The cow is deliberately passable in this prototype. It chooses a random destination within the wandering radius, walks there, then idles. It pauses while inspected.

Door zones use their transform's local rectangle; select one to see the cyan bounds. They do not require physics triggers. For an additional building, create a paired entrance/exit with clear spawn markers, assign its indoor root as the entrance's Destination Area and exit's Departing Area, and assign the corresponding spawn Transform to each Destination. Keep the player, camera, clock, transition component, and fade canvas **outside** any area that gets deactivated. Outdoor doorway zones and return markers sit just below the existing solid building colliders.

## Runtime behavior and extension points

- `FarmInteractable` provides registration, range checks, prompt text, input kind, and interaction dispatch. `PlayerInteraction` selects only the nearest in-range object and reads button presses from a private copy of the project's Input Actions asset.
- The new `FarmInteraction` map adds **Inspect/E**, **Door/F**, and **Close/Escape**. The original `Player` and `UI` maps remain unchanged. The new UI EventSystem uses the existing UI actions and `InputSystemUIInputModule`.
- `AnimalStats` owns the stats independently of the panel and wandering. Each cow has its own component instance. Hunger loses **2 points per game hour** by default, clamps to 0–100, and uses absolute elapsed game minutes to handle midnight or accelerated time. It does not decline when the clock is paused or at the Day 30 cap. Time and hunger continue while inspecting and while indoors. There is no starvation damage or automatic behavior escalation.
- `Normal`, `Nervous`, and `Aggressive` are descriptive enum values available for later extensions; only normal wandering is implemented. `SetStatus` permits future behavior systems to update clamped stats without controlling UI. No anomaly behavior is included.
- `PlayerMovement.SetMovementLocked(owner, locked)` uses independent ownership for the modal and transition locks. Closing a panel cannot release another system's lock.
- `BuildingTransition` fades out, activates the destination, teleports the existing Rigidbody2D player, snaps the existing camera, holds a fully black screen for **at least 0.5 real seconds**, then fades in. It rejects repeated interaction during the transition. Fades use real time, including when the game clock is paused.
- Farmhouse and barn interiors are separate, distant areas in the **same scene**, enabled on entry and disabled on exit. The outdoor farm stays active, so the clock, cow stats, and wandering continue. This preserves state across building transitions without a save system or scene singleton. Stopping Play resets runtime state normally.
- Interior sprites use the existing Global Light/day-night cycle. There are no extra global lights or cameras.

To add another cow, duplicate **Cow - Clover** and assign its name, stats, and grazing marker/size. The shared panel displays the selected cow's own stats.

## Practical testing checklist

- [ ] Confirm the Console has no compile errors, run the setup menu, then run it again: one cow, one interaction UI, and two interiors remain.
- [ ] Verify existing WASD/arrow movement, smooth camera, boundary/building collisions, clock HUD, and day/night lighting still work.
- [ ] Watch the cow idle and walk. Try high speed, large wandering radius, and a smaller valid pasture: the entire sprite remains inside the selected green rectangle. Try pushing it: the player passes through it.
- [ ] Approach and leave the cow: the E prompt appears/disappears at the configured radius. Press E: name and all four status values appear; Behavior defaults to Normal.
- [ ] Hold movement keys while opening the panel: the player stops immediately and the cow pauses. Close via mouse, reopen, and close via Escape. Movement resumes and the panel does not reopen from a held E key.
- [ ] Set Minutes Per Second to 60: one real second is one game hour, so hunger drops about 2 points/second. Set speed to 0: hunger stays unchanged. Test across midnight and verify hunger never falls below 0.
- [ ] Test each building independently. Approach its south door, press/hold/spam F: only one transition runs at a time. The screen fully covers the HUD, remains black for at least 0.5 seconds, then reveals the correct room and entrance.
- [ ] Walk against all four indoor walls and the placeholder table. At the south exit, press F: return to that building's own outdoor entrance. Hold F through the transition: it must not immediately enter again.
- [ ] Record day/time/hunger, spend time indoors, then return and inspect the cow. Time and hunger should have progressed rather than reset. Confirm camera movement does not sweep across the gap between areas.
- [ ] Try both transitions with Minutes Per Second at 0. Fade and movement work while the clock remains paused.
- [ ] Duplicate the cow, change its name/stats and position within its pasture, then inspect each. Verify selection and stats remain independent.
- [ ] Restore test settings, save, and repeat the core flow in a standalone build of FarmPrototype.

## Automated verification

Run `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/ValidateFarmScripts.ps1` after Unity has imported the packages. This enables script execution only for that PowerShell process, without changing the machine policy. Override `-UnityData` if Unity is installed elsewhere. This compiles runtime and Editor code separately against the installed Unity/package assemblies and checks seven hunger edge cases plus 10,000 pasture clamp/idempotence cases. It writes only to ignored `Temp/FarmValidation` and uses Unity's bundled compiler/runtime.

These checks do not simulate Unity physics, UI events, scene setup, or rendered fades. Use the play-mode checklist for those behaviors.
