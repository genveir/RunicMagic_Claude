# RunicMagic_Claude Todo

## To Do — Milestone 2

Next ticket number: RMC-98
Next bugfix number: BUG-7

| Key | Title | Description | Blocked By |
|-----|-------|-------------|------------|
| RMC-77 | World AI system | Add an `AiCapability` to the entity model. World exposes `TickAi(deltaSeconds)` which iterates all entities with AI and calls their tick. | |
| RMC-80 | PatrolAi behavior | First concrete AI behavior: `PatrolAiCapability` — walks an entity back and forth along a list of waypoints at a configurable speed. Needs DB schema for waypoints and speed. | RMC-77 |
| RMC-83 | Movement service | Introduce a movement service that sits between any "I want to move" request (AI, future systems) and the actual position update. The service is the single place that knows about ongoing motion effects and gates or modifies autonomous movement accordingly. For now: an entity under an active motion effect cannot produce its own movement. | RMC-82 RMC-77 |
| RMC-79 | Guard NPC scenario | Add a guard entity to the test world with a patrol path along a visible corridor. The guard should be visually distinct. | RMC-80 RMC-78 RMC-83 |
| RMC-75 | 🏁 Milestone 3 — Have a guard walk by and get pushed | Implement a simple guard NPC that walks back and forth along a predefined path and push him away with a DAN-targeted spell. This will require the engine to have a concept of time. | RMC-79 RMC-82 |

## To Do — Other

| Key | Title | Description | Blocked By |
|-----|-------|-------------|------------|
| RMC-96 | Remove primary constructors | Find all non-record classes and structs that use primary constructors and replace them with explicit constructor bodies. Fields should be declared separately. Records may keep primary constructors. | |
| RMC-95 | Clean up GlobalUsings in test project | `GlobalUsings.cs` currently imports `RunicMagic.World.Capabilities` and `RunicMagic.World.Runes.RuneTypes` — both are narrow domain namespaces that should be explicit where needed. Move them out. Add `FluentAssertions` and `Xunit` as global usings instead, since every test file needs them. | |
| RMC-91 | Move entry point to Controller; invert View/Controller dependency | Currently View hosts the application, making Controller a dependency of View. This inverts the intended hub-and-spokes architecture. Move the entry point into Controller so that Controller owns the host and View becomes a spoke that Controller depends on, not the other way around. | |
| RMC-92 | Move status rendering to View | The terminal prompt is currently rendered in the Controller assembly. Controller should instead produce a model carrying the caster's status information (e.g. power, health) and pass it to View, which is responsible for deciding how to format and display that data. | |
| RMC-60 | Design small items | Define the world model for small items — portable objects a creature can carry (e.g. a mana gem in the caster's pocket). Covers how items are represented, how carrying/inventory works, and how items interact with spells and power sourcing. | |
| RMC-37 | Kill creatures when they run out of life | At any point during spell execution, living entities may run out of hitpoints. In this case the game should register that they're dead and remove their living and agency properties | |
| RMC-38 | Register entity destruction | It should be possible to destroy entities. When this happens, the game should register that the entity is destroyed | |
| RMC-39 | Stop spell execution on caster or executor death or destruction | When the caster or executor of a spell cease to be in an active state the spell should stop executing | RMC-37 RMC-38 |
| RMC-14 | Design channeling and persistent effects | Written runes stay active while power is channeled. Define what "channeling" means mechanically — what keeps a spell alive, how it is terminated, and how the executor tracks ongoing effects. | |
| RMC-48 | Formalize movement with collision | VUN currently teleports entities to their destination. Replace this with movement through space: the entity travels along the push vector and stops when it hits an entity (a wall, door, etc.) rather than passing through it. | |
| RMC-74 | Cone selection rune | A spatial selector that selects all entities within a cone projected from an origin entity in its pointing direction. Parametrised by angle (half-width) and range. | |
| BUG-4 | Up/Down arrow in terminal doesn't reprint prompt | When using the up or down arrow to redo earlier commands in the terminal, it cuts into the prompt | |
| RMC-84 | Partial view updates via SSE | Instead of posting the full world render on every game tick, post only the entities that changed during that tick. The client merges the delta into its current render state rather than replacing it wholesale. | RMC-78 |

## In Progress
| Key | Title | Description | Remarks |
|-----|-------|-------------|---------|

## Ready For Review

| Key | Title |
|-----|-------|

## Done

| Key | Title |
|-----|-------|
| RMC-76 | Game clock |
| RMC-78 | SSE world push |
| RMC-81 | Queue spells for tick processing |
| RMC-82 | Motion effects queue |
| RMC-85 | Scale up power costs |
| RMC-86 | EventTracker — World-layer rename and entity tracking |
| RMC-93 | ControllerEvent hierarchy |
| RMC-94 | Game loop refactor |
| RMC-88 | Clean up TryAdvance/TryAdvanceInner split |
| RMC-89 | YI/SA decorator runes and live vs calcified evaluation |
| RMC-97 | Update design docs to reflect ongoing effects |
| BUG-6 | Malformed rune names produce no output |
| BUG-5 | Entity clicks break during canvas animation |
| RMC-90 | Move linear motion cost calculation into LinearMotionEffect |