# RunicMagic_Claude Todo

## Note for models using this file:

Most tickets in this file are written by an assistant with incomplete information about the project and its current state. Treat the tickets as unrefined user stories the user wants to see implemented, rather than polished tasks ready for development.

The Key column in every table uses right-padded cells. The baseline is 7 characters (RMC-NNN). When inserting a row, pad the key with trailing spaces to reach 7 characters before the closing "|"

## To Do — Milestone 4 — Kill the guard before he attacks

Next ticket number: RMC-128
Next bugfix number: BUG-11

| Key     | Title | Description | Blocked By |
|---------|-------|-------------|------------|
| RMC-98  | Velocity-imparting runes | Runes that impart velocity to entities in a Set, handing them off to the simulation layer for physics-driven motion. Contrast with VUN/VAR/CJIR/CJAR which are engine-layer instructions that bypass physics. | |
| RMC-115 | Encapsulate life and structural integrity mutation on Entity | Life and structural integrity must only be altered through dedicated methods on `Entity` — never by directly setting the backing field or property. Audit all sites that currently write these values and replace them with method calls. | |
| RMC-37  | Kill creatures when they run out of life | At any point during spell execution, living entities may run out of hitpoints. In this case the game should register that they're dead and remove their living and agency properties. | RMC-115 |
| RMC-38  | Register entity destruction | It should be possible to destroy entities. When this happens, the game should register that the entity is destroyed. | RMC-115 |
| RMC-60  | Small items | Physical entities that a creature can carry (e.g. a club or a mana gem). Carrying means the item moves with the carrier. Covers how items are represented and how carrying works. | |
| RMC-122 | Hardness property | Add an explicit hardness property to Entity. Governs how much collision damage an entity deals and absorbs on impact. Distinct from density (weight/surface), which does not fully substitute for the material resistance of the 3D objects these 2D entities represent. | |
| RMC-48  | Motion with collision | Simulation-moving entities detect and respond to collisions. Both parties take damage proportional to collision energy. | RMC-122 |
| RMC-123 | Strike capability | An entity can have arms: defined by arm length, swing arc, and acceleration. A strike swings a carried item through the arc; the collision system handles impact damage on contact. | |
| RMC-124 | Vision capability | An entity can perceive other entities within a configurable range and cone. Structured similarly to LocomotionCapability. | |
| RMC-125 | Aggressive AI behavior | When the guard detects the caster via vision, pursue and strike. Includes vision strategies and fighting strategies. | RMC-123 RMC-124 |
| RMC-126 | Seed aggressive guard | Add a new guard entity (distinct from the patrol guard) inside the building, equipped with a club. | RMC-48 RMC-60 RMC-122 RMC-123 RMC-124 RMC-125 |
| RMC-127 | 🏁 Milestone 4 — The caster smashes the aggressive guard into the wall with a spell hard enough to kill him before he can attack | | RMC-126 |

## To Do — Other

| Key     | Title | Description | Blocked By |
|---------|-------|-------------|------------|
| RMC-100 | In-game entity creation and modification | Add the ability to create new entities and modify existing ones while the game is running — without restarting or editing seed data. This is a prerequisite for conveniently populating the world during development. | |
| RMC-101 | Save game state to database | Persist current world state (entities and their properties) to the database. | |
| RMC-14  | Design channeling and persistent effects | Written runes stay active while power is channeled. Define what "channeling" means mechanically — what keeps a spell alive, how it is terminated, and how the executor tracks ongoing effects. | |
| RMC-74  | Cone selection rune | A spatial selector that selects all entities within a cone projected from an origin entity in its pointing direction. Parametrised by angle (half-width) and range. | |
| RMC-117 | Right-click move on canvas | Right-clicking a position on the canvas should move the caster to that location. | |

## In Progress
| Key | Title | Description | Remarks |
|-----|-------|-------------|---------|

## Ready For Review

| Key | Title | Description |
|-----|-------|-------------|

## Done

| Key     | Title |
|---------|-------|