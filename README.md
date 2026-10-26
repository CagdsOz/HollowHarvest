# HollowHarvest

A top-down pixel-art farming horror prototype using Unity 6000.5.5f1 and Universal 2D (URP).

## Cow interaction and buildings

After importing the scripts, run **HollowHarvest > Add Cow and Building Interactions** to extend the existing farm. Approach the cow in the southeast pasture and press **E**; close its status panel with **Escape** or the Close button. Approach the south door of the farmhouse or barn and press **F** to enter; use **F** at the indoor exit to return.

See [cow/building setup and testing](Docs/CowAndBuildings.md) for Inspector settings, generated assets, architecture, and the practical acceptance checklist.

## Launch Sprint 1

1. Open this project with Unity **6000.5.5f1** and wait for script compilation. If Unity was already open, use **Assets > Refresh**.
2. Choose **HollowHarvest > Create Sprint 1 Farm Scene** from the top menu. Save any currently edited scene when Unity asks.
3. The command creates and opens `Assets/Scenes/FarmPrototype.unity`, connects every component, saves placeholder sprites in `Assets/Art/Prototype`, and places the scene first in Build Settings. Subsequent invocations open the existing scene without rebuilding it.
4. Press **Play**, click the Game view, and move with **WASD** or arrow keys. The existing action also supports a gamepad stick.

The scene is generated in the Editor; it is not generated on entering Play mode. No manual component wiring or package installation is needed. The original SampleScene remains available. To build the prototype, open **File > Build Profiles**, confirm FarmPrototype is the first enabled scene in the Scene List, then build for your installed target platform.

## Inspector settings

- **Player > Player Movement:** Move Speed (default 4 units/second). Uses a private copy of the existing `InputSystem_Actions` asset's `Player/Move` action. The project already has Input System-only handling enabled.
- **Player > Rigidbody 2D:** zero gravity, frozen rotation, interpolation, and continuous collision detection. Movement sets Unity 6 `linearVelocity` during FixedUpdate. Diagonal input is clamped to the same maximum speed.
- **Main Camera > Camera Follow:** Target, Offset, Smooth Time. Camera movement runs in LateUpdate.
- **Game Clock > Game Clock:** Starting Day/Hour/Minute (1 / 6 / 0) and Minutes Per Second (default 1). A full day lasts 24 real minutes at speed 1. Speed 0 pauses the clock; speed 60 makes a day last 24 seconds. Play-mode changes are temporary.
- **Global Light 2D > Day Night Lighting:** clock reference, color gradient, and intensity curve across normalized time (0 = midnight, 0.25 = 06:00, 0.5 = noon, 0.75 = 18:00, 1 = midnight). Keep the endpoints equal for seamless midnight lighting. All world sprites use URP's Sprite-Lit-Default material; the overlay HUD stays readable at night.

The clock handles midnight and multi-day jumps, stops at **Day 30, 23:59**, and displays a completion message. Movement remains available. This is only a prototype endpoint, with no survival/win/lose system.

## Play-mode acceptance checks

1. Enter Play: HUD reads Day 01 / 30, 06:00. Walk in all directions and diagonally. Release the keys and confirm the player stops. Camera follows smoothly.
2. Walk into all four fences, the farmhouse, barn, and well: they block movement. Soil plots and paths remain walkable.
3. Set Minutes Per Second to 60 during Play. Observe daylight, dusk, night, dawn, and the midnight day rollover. Set speed to 0 and confirm both the time and lighting stop changing while movement continues.
4. Stop Play. Set Starting Hour to 23 and Starting Minute to 59, leaving Starting Day at 1. Enter Play and verify rollover to Day 2, 00:00.
5. Stop Play. Set Starting Day to 30, Hour to 23, Minute to 58, and speed to 60. Enter Play: the clock stops at 30 / 23:59 and shows completion. It never advances to Day 31.
6. Restore starting values to **1 / 6 / 0** and speed to **1**, save the scene, and confirm the Console has no errors. Test a standalone build using the same movement and clock checks.

## Source files

- `Assets/Scripts/PlayerMovement.cs`: Input System and Rigidbody2D movement.
- `Assets/Scripts/CameraFollow.cs`: smooth top-down camera.
- `Assets/Scripts/GameClock.cs`: configurable 24-hour clock and 30-day cap; exposes a TimeChanged event for future consumers.
- `Assets/Scripts/DayNightLighting.cs`: continuous URP global light color/intensity updates.
- `Assets/Scripts/ClockHud.cs`: day/time display using the installed Unity UI package.
- `Assets/Editor/PrototypeSceneBuilder.cs`: repeatable, Editor-only farm setup and procedural placeholder sprite creation.

Sprint 1 itself contains no animal systems. The cow/building extension above adds normal cow inspection and indoor areas; anomalies, enemies, inventory, economy, and farming interactions remain out of scope. No additional packages are required.
