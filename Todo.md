# RunicMagic_Claude Todo

## Note for models using this file:

Most tickets in this file are written by an assistant with incomplete information about the project and its current state. Treat the tickets as unrefined user stories the user wants to see implemented, rather than polished tasks ready for development.

The Key column in every table uses right-padded cells. The baseline is 7 characters (RMC-NNN). When inserting a row, pad the key with trailing spaces to reach 7 characters before the closing "|"

## To Do — Milestone 3.5 — Maintenance

| Key | Title | Description | Blocked By |
|-----|-------|-------------|------------|
| RMC-91  | Move entry point to Controller; invert View/Controller dependency | Currently View hosts the application, making Controller a dependency of View. This inverts the intended hub-and-spokes architecture. Move the entry point into Controller so that Controller owns the host and View becomes a spoke that Controller depends on, not the other way around. | |
| RMC-84  | Partial view updates via SSE | Instead of posting the full world render on every game tick, post only the entities that changed during that tick. The client merges the delta into its current render state rather than replacing it wholesale. | RMC-78 |
| RMC-92  | Move status rendering to View | The terminal prompt is currently rendered in the Controller assembly. Controller should instead produce a model carrying the caster's status information (e.g. power, health) and pass it to View, which is responsible for deciding how to format and display that data. | |
| RMC-109 | Split WorldModel into entity store, motion queue, and ticker | `WorldModel` currently has three responsibilities: entity storage, engine motion effect queuing, and tick orchestration. Split into: `WorldModel` (pure entity store and spatial queries), `EngineMotionQueue` (owns `_engineMotionEffects` and `AddMotionEffect`; `TickEngineMotion` moves here), and `WorldTicker` (tick orchestrator, takes the other two as dependencies and drives `TickEntities`/`TickAI`/`TickPhysics`). Retire `IGameLoopWorldModel`. | |
| RMC-111 | Split movement policy out of LocomotionCapability | `LocomotionCapability` currently conflates two concerns: the physical model (legs, offsets, efficiency, how to produce `ForceVector` impulses) and movement policy (goal-seeking, braking decisions, turn-vs-walk priority). Split these so that `LocomotionCapability` exposes physical primitives only: `ApplyForwardForce(entity, fraction)`, `ApplyTurnForce(entity, direction, fraction)`, and query methods like `GetLinearBrakingDistance(entity, forceFraction)` / `GetAngularBrakingAngle(entity, forceFraction)` (which delegate to `PhysicsService` internally). The current goal-seeking logic (`ApplyLocomotion`, `ApplyWalking`, `ApplyTurning`, braking simulations) moves into AI behaviour classes in the AI layer. This allows behaviours like sneak (10% force), combined turn+accelerate (split leg allocation), or custom stopping policy without touching the capability. | RMC-103 |
| RMC-96  | Remove primary constructors | Find all non-record classes and structs that use primary constructors and replace them with explicit constructor bodies. Fields should be declared separately. Records may keep primary constructors. | |
| RMC-102 | Remove defaults from EntityData; add test builder | `EntityData` properties currently have default values, which lets tests construct incomplete objects silently. Remove the defaults so the compiler enforces full initialisation, and introduce an `EntityDataBuilder` (or similar) in the test project to make constructing valid test instances convenient. | |
| RMC-104 | Normalise current/max parameter order | Audit all methods that take both a `current` and a `max` value of the same type and ensure `current` comes before `max` consistently across the codebase. | |
| RMC-106 | Change base motion constant from 56 to 70 ticks | The base motion constant used for motion timing is currently 56 ticks. Change it to 70. Also make it a constant in the technical sense. | |
| RMC-112 | Add Position type | Add a `Position` record to the geometry layer combining a `Location` and a facing `double Angle`. Represents a point in the world with an orientation — useful anywhere a destination also implies a facing direction (patrol waypoints, spawn points, etc.). | |
| RMC-113 | Patrol waypoints use Position | Replace the `Location` in `PatrolWaypoint` with a `Position`. When the guard arrives at a waypoint, it should adopt the waypoint's facing angle in addition to reaching its location. Requires updating `PatrolWaypointData` (add `Angle float` column to `PatrolWaypoints` table), `PatrolAIBehavior`, and the locomotion/turning logic to orient the entity at the destination. | RMC-112 |
| RMC-110 | Rewrite Strength as a capability | Extract `Strength` from its current form and rewrite it as a proper capability on `Entity`, following the same pattern as `LocomotionCapability`. | |
| RMC-107 | Show facing direction on canvas entities | Render a facing indicator (e.g. a line or arrow) on entities in the world canvas so their current angle is visually obvious. Useful for debugging locomotion and AI behavior. | |
| RMC-99  | Camera controls on the canvas | Replace the auto-fit viewBox (currently recalculated from the entity bounding box on every tick) with a persistent camera state. Scroll wheel zooms; click-and-drag pans. The keyboard stays reserved for spell input, so no WASD/arrow controls. Initial view on first entity load can still auto-fit, but after that the camera is user-controlled. | |

## To Do — Other

| Key | Title | Description | Blocked By |
|-----|-------|-------------|------------|
| RMC-100 | In-game entity creation and modification | Add the ability to create new entities and modify existing ones while the game is running — without restarting or editing seed data. This is a prerequisite for conveniently populating the world during development. | |
| RMC-101 | Save game state to database | Persist current world state (entities and their properties) to the database. | |
| RMC-60  | Design small items | Define the world model for small items — portable objects a creature can carry (e.g. a mana gem in the caster's pocket). Covers how items are represented, how carrying/inventory works, and how items interact with spells and power sourcing. | |
| RMC-37  | Kill creatures when they run out of life | At any point during spell execution, living entities may run out of hitpoints. In this case the game should register that they're dead and remove their living and agency properties | |
| RMC-38  | Register entity destruction | It should be possible to destroy entities. When this happens, the game should register that the entity is destroyed | |
| RMC-39  | Stop spell execution on caster or executor death or destruction | When the caster or executor of a spell cease to be in an active state the spell should stop executing | RMC-37 RMC-38 |
| RMC-14  | Design channeling and persistent effects | Written runes stay active while power is channeled. Define what "channeling" means mechanically — what keeps a spell alive, how it is terminated, and how the executor tracks ongoing effects. | |
| RMC-48  | Formalize movement with collision | VUN currently moves entities to their destination clipping through everything in its path. Replace this with movement through space: the entity travels along the push vector and collides when it hits an entity (a wall, door, etc.) rather than passing through it. | |
| RMC-74  | Cone selection rune | A spatial selector that selects all entities within a cone projected from an origin entity in its pointing direction. Parametrised by angle (half-width) and range. | |
| RMC-98  | Design velocity-imparting runes | Design runes that impart velocity and hand an entity off to the simulation layer, rather than directly rewriting its position (engine-layer). The key use case is battlefield shrapnel — objects flung into motion that then collide under physics rules, dealing damage. Contrast with VUN/VAR/CJIR/CJAR which are engine-layer instructions that bypass physics. | |

## In Progress
| Key | Title | Description | Remarks |
|-----|-------|-------------|---------|

## Ready For Review

| Key | Title | Description |
|-----|-------|-------------|

## Done

| Key | Title |
|-----|-------|
| BUG-4   | Up/Down arrow in terminal doesn't reprint prompt |
