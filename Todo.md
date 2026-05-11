# RunicMagic_Claude Todo

## Note for models using this file:

Most tickets in this file are written by an assistant with incomplete information about the project and its current state. Treat the tickets as unrefined user stories the user wants to see implemented, rather than polished tasks ready for development.

The Key column in every table uses right-padded cells. The baseline is 7 characters (RMC-NNN). When inserting a row, pad the key with trailing spaces to reach 7 characters before the closing "|"

## To Do — Milestone 3.5 — Maintenance

Next ticket number: RMC-122
Next bugfix number: BUG-11

| Key | Title | Description | Blocked By |
|-----|-------|-------------|------------|
| RMC-99  | Camera controls on the canvas | Replace the auto-fit viewBox (currently recalculated from the entity bounding box on every tick) with a persistent camera state. Scroll wheel zooms; click-and-drag pans. The keyboard stays reserved for spell input, so no WASD/arrow controls. Initial view on first entity load can still auto-fit, but after that the camera is user-controlled. | |
| RMC-120 | Add locomotion to EntityBuilder | `EntityBuilder` should expose a fluent method for configuring locomotion so callers don't have to construct a `LocomotionCapability` manually. Currently test helpers and world models build `LocomotionCapability` (with its `Leg` list and efficiency value) outside the builder and pass the assembled object in via `WithLocomotion()`. The builder should accept the locomotion parameters directly and own the assembly. | |

## To Do — Other

| Key | Title | Description | Blocked By |
|-----|-------|-------------|------------|
| RMC-100 | In-game entity creation and modification | Add the ability to create new entities and modify existing ones while the game is running — without restarting or editing seed data. This is a prerequisite for conveniently populating the world during development. | |
| RMC-101 | Save game state to database | Persist current world state (entities and their properties) to the database. | |
| RMC-60  | Design small items | Define the world model for small items — portable objects a creature can carry (e.g. a mana gem in the caster's pocket). Covers how items are represented, how carrying/inventory works, and how items interact with spells and power sourcing. | |
| RMC-115 | Encapsulate life and structural integrity mutation on Entity | Life and structural integrity must only be altered through dedicated methods on `Entity` — never by directly setting the backing field or property. Audit all sites that currently write these values and replace them with method calls. | |
| RMC-37  | Kill creatures when they run out of life | At any point during spell execution, living entities may run out of hitpoints. In this case the game should register that they're dead and remove their living and agency properties | RMC-115 |
| RMC-38  | Register entity destruction | It should be possible to destroy entities. When this happens, the game should register that the entity is destroyed | RMC-115 |
| RMC-39  | Stop spell execution on caster or executor death or destruction | When the caster or executor of a spell cease to be in an active state the spell should stop executing | RMC-37 RMC-38 |
| RMC-14  | Design channeling and persistent effects | Written runes stay active while power is channeled. Define what "channeling" means mechanically — what keeps a spell alive, how it is terminated, and how the executor tracks ongoing effects. | |
| RMC-48  | Formalize movement with collision | VUN currently moves entities to their destination clipping through everything in its path. Replace this with movement through space: the entity travels along the push vector and collides when it hits an entity (a wall, door, etc.) rather than passing through it. | |
| RMC-74  | Cone selection rune | A spatial selector that selects all entities within a cone projected from an origin entity in its pointing direction. Parametrised by angle (half-width) and range. | |
| RMC-98  | Design velocity-imparting runes | Design runes that impart velocity and hand an entity off to the simulation layer, rather than directly rewriting its position (engine-layer). The key use case is battlefield shrapnel — objects flung into motion that then collide under physics rules, dealing damage. Contrast with VUN/VAR/CJIR/CJAR which are engine-layer instructions that bypass physics. | |
| RMC-117 | Right-click move on canvas | Right-clicking a position on the canvas should move the caster to that location. | |

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
| RMC-91  | Move entry point to Controller; invert View/Controller dependency |
| BUG-9   | VUN(push) with distance 0 does not execute |
| RMC-92  | Move status rendering to View |
| RMC-114 | Add life and power bars to the caster HUD |
| BUG-10  | Dead casters can cast spells |
| RMC-109 | Split WorldModel into entity store, motion queue, and ticker |
| RMC-96  | Remove primary constructors |
| RMC-116 | Remove underscore prefixes from private fields |
| RMC-104 | Normalise current/max parameter order |
| RMC-102 | Remove defaults from EntityData |
| RMC-111 | Split movement policy out of LocomotionCapability |
| RMC-107 | Show facing direction on canvas entities |
| RMC-118 | Refactor app.js SVG rendering |
| RMC-106 | Change base motion constant from 56 to 98 ticks |
| RMC-112 | Add Position type |
| RMC-113 | Patrol waypoints use Position |
| RMC-119 | Store angles as compass radians, convert in WorldLoader |
| RMC-110 | Rewrite Strength as a capability |
| RMC-121 | Rename StructuralIntegrityCapability |