# RunicMagic_Claude Todo

## Note for models using this file:

Most tickets in this file are written by an assistant with incomplete information about the project and its current state. Treat the tickets as unrefined user stories the user wants to see implemented, rather than polished tasks ready for development.

The Key column in every table uses right-padded cells. The baseline is 7 characters (RMC-NNN). When inserting a row, pad the key with trailing spaces to reach 7 characters before the closing "|"

## To Do — Milestone 2

Next ticket number: RMC-108
Next bugfix number: BUG-8

| Key | Title | Description | Blocked By |
|-----|-------|-------------|------------|
| RMC-105 | Simulation-layer physics service | Implement the simulation layer for entity motion. Add a `VelocityVector?` field to `Entity` (null = stationary). Implement a physics service that each tick integrates velocity, applied forces (drive force from locomotion or a launch impulse), and drag, then writes updated `Location` and velocity back to the entity. Entities where `IsUnderEngineMotion` is true are skipped — the engine layer has them and physics does not negotiate. Covers both self-propelled entities (a guard walking) and passive ones (a rock in flight, an entity sliding on ice). The service has no concept of creature intent; that belongs to `LocomotionCapability`. | |
| RMC-103 | LocomotionCapability | Add `LocomotionCapability` to `Entity` (already stubbed as a nullable property). The capability carries the bio-mechanical properties that only self-propelling entities have: strength, locomotion efficiency, drag coefficient, traction coefficient, CoM/stance width ratio, and gait inertia. Each tick, it computes a drive force from the entity's current intent (desired direction and gait, set by AI or player) and submits it to the physics service. Max speed is not stored — it emerges from physics. A rock, a wall, or a plant has no `LocomotionCapability`. | RMC-105 |
| RMC-80  | PatrolAi behavior | First concrete AI behavior: `PatrolAiBehavior` — walks an entity back and forth along a list of waypoints. Sets locomotion intent (desired direction) each tick; the physics service and `LocomotionCapability` handle the actual motion. Needs DB schema for waypoints. | RMC-77, RMC-103, RMC-105 |
| RMC-75  | 🏁 Milestone 3 — Have a guard walk by and get pushed | Implement a simple guard NPC that walks back and forth along a predefined path and push him away with a DAN-targeted spell. This will require the engine to have a concept of time. | RMC-83 |

## To Do — Other

| Key | Title | Description | Blocked By |
|-----|-------|-------------|------------|
| RMC-107 | Show facing direction on canvas entities | Render a facing indicator (e.g. a line or arrow) on entities in the world canvas so their current angle is visually obvious. Useful for debugging locomotion and AI behavior. | |
| RMC-106 | Change base motion constant from 56 to 70 ticks | The base motion constant used for motion timing is currently 56 ticks. Change it to 70. Also make it a constant in the technical sense. | |
| RMC-100 | In-game entity creation and modification | Add the ability to create new entities and modify existing ones while the game is running — without restarting or editing seed data. This is a prerequisite for conveniently populating the world during development. | |
| RMC-101 | Save game state to database | Persist current world state (entities and their properties) to the database. | |
| RMC-96  | Remove primary constructors | Find all non-record classes and structs that use primary constructors and replace them with explicit constructor bodies. Fields should be declared separately. Records may keep primary constructors. | |
| RMC-91  | Move entry point to Controller; invert View/Controller dependency | Currently View hosts the application, making Controller a dependency of View. This inverts the intended hub-and-spokes architecture. Move the entry point into Controller so that Controller owns the host and View becomes a spoke that Controller depends on, not the other way around. | |
| RMC-92  | Move status rendering to View | The terminal prompt is currently rendered in the Controller assembly. Controller should instead produce a model carrying the caster's status information (e.g. power, health) and pass it to View, which is responsible for deciding how to format and display that data. | |
| RMC-60  | Design small items | Define the world model for small items — portable objects a creature can carry (e.g. a mana gem in the caster's pocket). Covers how items are represented, how carrying/inventory works, and how items interact with spells and power sourcing. | |
| RMC-37  | Kill creatures when they run out of life | At any point during spell execution, living entities may run out of hitpoints. In this case the game should register that they're dead and remove their living and agency properties | |
| RMC-38  | Register entity destruction | It should be possible to destroy entities. When this happens, the game should register that the entity is destroyed | |
| RMC-39  | Stop spell execution on caster or executor death or destruction | When the caster or executor of a spell cease to be in an active state the spell should stop executing | RMC-37 RMC-38 |
| RMC-14  | Design channeling and persistent effects | Written runes stay active while power is channeled. Define what "channeling" means mechanically — what keeps a spell alive, how it is terminated, and how the executor tracks ongoing effects. | |
| RMC-48  | Formalize movement with collision | VUN currently moves entities to their destination clipping through everything in its path. Replace this with movement through space: the entity travels along the push vector and collides when it hits an entity (a wall, door, etc.) rather than passing through it. | |
| RMC-74  | Cone selection rune | A spatial selector that selects all entities within a cone projected from an origin entity in its pointing direction. Parametrised by angle (half-width) and range. | |
| BUG-4   | Up/Down arrow in terminal doesn't reprint prompt | When using the up or down arrow to redo earlier commands in the terminal, it cuts into the prompt | |
| RMC-84  | Partial view updates via SSE | Instead of posting the full world render on every game tick, post only the entities that changed during that tick. The client merges the delta into its current render state rather than replacing it wholesale. | RMC-78 |
| RMC-98  | Design velocity-imparting runes | Design runes that impart velocity and hand an entity off to the simulation layer, rather than directly rewriting its position (engine-layer). The key use case is battlefield shrapnel — objects flung into motion that then collide under physics rules, dealing damage. Contrast with VUN/VAR/CJIR/CJAR which are engine-layer instructions that bypass physics. | |
| RMC-102 | Remove defaults from EntityData; add test builder | `EntityData` properties currently have default values, which lets tests construct incomplete objects silently. Remove the defaults so the compiler enforces full initialisation, and introduce an `EntityDataBuilder` (or similar) in the test project to make constructing valid test instances convenient. | |
| RMC-104 | Normalise current/max parameter order | Audit all methods that take both a `current` and a `max` value of the same type and ensure `current` comes before `max` consistently across the codebase. | |
| RMC-99  | Camera controls on the canvas | Replace the auto-fit viewBox (currently recalculated from the entity bounding box on every tick) with a persistent camera state. Scroll wheel zooms; click-and-drag pans. The keyboard stays reserved for spell input, so no WASD/arrow controls. Initial view on first entity load can still auto-fit, but after that the camera is user-controlled. | |

## In Progress
| Key | Title | Description | Remarks |
|-----|-------|-------------|---------|

## Ready For Review

| Key | Title | Description |
|-----|-------|-------------|

## Done

| Key | Title |
|-----|-------|
| RMC-76  | Game clock |
| RMC-78  | SSE world push |
| RMC-81  | Queue spells for tick processing |
| RMC-82  | Motion effects queue |
| RMC-85  | Scale up power costs |
| RMC-86  | EventTracker — World-layer rename and entity tracking |
| RMC-93  | ControllerEvent hierarchy |
| RMC-94  | Game loop refactor |
| RMC-88  | Clean up TryAdvance/TryAdvanceInner split |
| RMC-89  | YI/SA decorator runes and live vs calcified evaluation |
| RMC-97  | Update design docs to reflect ongoing effects |
| BUG-6   | Malformed rune names produce no output |
| BUG-5   | Entity clicks break during canvas animation |
| RMC-90  | Move linear motion cost calculation into LinearMotionEffect |
| RMC-95  | Clean up GlobalUsings in test project |
| RMC-79  | Guard NPC scenario |
| RMC-77  | World AI system |
| RMC-83  | Locomotion service |
| BUG-7   | Prompt printed on every tick |