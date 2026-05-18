# RunicMagic_Claude Todo

## Note for models using this file:

Most tickets in this file are written by an assistant with incomplete information about the project and its current state. Treat the tickets as unrefined user stories the user wants to see implemented, rather than polished tasks ready for development.

The Key column in every table uses right-padded cells. The baseline is 7 characters (RMC-NNN). When inserting a row, pad the key with trailing spaces to reach 7 characters before the closing "|"

Next ticket number: RMC-149
Next bugfix number: BUG-11

## To Do - Milestone 4 - Decomposed runes
| Key     | Title | Description | Blocked By |
|---------|-------|-------------|------------|
| RMC-148 | Filtered GetFirstInRay | `DAN(pointing at)` and `KAL(touching/indicating)` both need "first entity on a ray satisfying a predicate" — currently both runes call `GetAllInRay` and filter in the rune body (skip caster, skip translucent), which prevents the engine from stopping the ray early and forces selection-cost overrides. A native `GetFirstInRay(filter)` would fix this, but expressing the filter in terms of the magic system's type model is non-trivial: the filter is conceptually an `IEntitySet → IEntitySet` transform, and rune subtrees produce values, not functions over values. Decide what form the filter should take. | |
| RMC-132 | Decompose filter runes into Property type | Introduce a Property rune type and decompose the monolithic property-selector runes into a Property root and shared filter/selector runes. See Design/FilterRunes.md. | RMC-139 |
| RMC-133 | Lift parser type grammar to full type expressions | Generalise the parser so that a single rune can satisfy a span of N consecutive expected argument types by producing exactly those N types in order. Required for multi-value runes such as DAR(pointing direction). | |
| RMC-134 | Add DAR(pointing direction) rune | DAR is a EntitySet -> [Location, Location] rune: consumes one EntitySet from the token stream and produces their averaged pointing directions. | RMC-133 |
| RMC-137 | Add ISTRANSPARENT filter rune | Filters a Set to entities that are transparent — i.e. do not occlude a ray passing through them. Required for DAN(pointing at) decomposition, where the ray must pass through windows and similar entities to reach the intended target. | |
| RMC-135 | Decompose DAN(pointing at) | Introduce RAY as a property root (along-ray distance, engine-native). Decompose DAN as a shorthand for HE RAY DAR. | RMC-132 RMC-134 RMC-137 |
| RMC-142 | Update VUN and VAR to take a direction | Replace the single origin Location argument on VUN(push) and VAR(pull) with two Location arguments expressing a direction: directionFrom (default PAR OH) and directionTo (default PAR of the target Set). Consistent with the direction-as-two-locations design established for ANG and RAY. | |
| RMC-74  | Cone selection rune | Either introduce a cone selection rune (CJODAN?) or decide that it must be expressed as a composition of more primitive runes. The cone is defined by an origin, a direction, and an angle. The direction is the ray from the origin to the point on the unit circle in the caster's pointing direction (DAR). The angle is a property of the target entities: their angular difference from that ray (ANG). | RMC-132 RMC-133 RMC-134 |
| RMC-129 | Cone angle guide overlay | A toolbar-toggle button that shows a cone guide on the caster: the pointing direction as a centerline, a half-circle arc, and 14 evenly-spaced radial division lines so the player can read off cone angles in base-14. Rendered as SVG overlaid on the caster entity. | RMC-74 |
| RMC-144 | Seed sheep and pasture | Add a small flock of sheep (living entities) behind the rotated wall near the caster. The sheep must not be reachable by DAN from the caster's starting position — the wall occludes line-of-sight. | |
| RMC-145 | 🏁 Milestone 4 — Herd the sheep through the gate | The caster selects living entities in a cone behind the wall and pushes them toward a nearby gate using a directional push. | RMC-135 RMC-136 RMC-142 RMC-144 |

## To Do — Milestone 5 — Kill the guard before he attacks

| Key     | Title | Description | Blocked By |
|---------|-------|-------------|------------|
| RMC-98  | Velocity-imparting runes | Runes that impart velocity to entities in a Set, handing them off to the simulation layer for physics-driven motion. Contrast with VUN/VAR/CJIR/CJAR which are engine-layer instructions that bypass physics. | |
| RMC-115 | Encapsulate life and structural integrity mutation on Entity | Life and structural integrity must only be altered through dedicated methods on `Entity` — never by directly setting the backing field or property. Audit all sites that currently write these values and replace them with method calls. | |
| RMC-37  | Kill creatures when they run out of life | At any point during spell execution, living entities may run out of hitpoints. In this case the game should register that they're dead and remove their living and agency properties. | RMC-115 |
| RMC-38  | Register entity destruction | It should be possible to destroy entities. When this happens, the game should register that the entity is destroyed. | RMC-115 |
| RMC-60  | Small items | Physical entities that a creature can carry (e.g. a club or a mana gem). Carrying means the item moves with the carrier. Covers how items are represented and how carrying works. | |
| RMC-122 | Hardness property | Add an explicit hardness property to Entity. Governs how much collision damage an entity deals and absorbs on impact. Distinct from density (weight/surface), which does not fully substitute for the material resistance of the 3D objects these 2D entities represent. | |
| RMC-130 | Collision detection service | A service that, given the world state, detects when two entities overlap. Returns collision pairs with enough data (positions, masses, velocities) for any collision responder to act on. Used by both the simulation layer and the engine layer, which respond differently. | |
| RMC-48  | Motion with collision | Simulation-moving entities respond to detected collisions. Both parties take damage proportional to collision energy. | RMC-122 RMC-130 |
| RMC-123 | Strike capability | An entity can have arms: defined by arm length, swing arc, and acceleration. A strike swings a carried item through the arc; the collision system handles impact damage on contact. | |
| RMC-124 | Vision capability | An entity can perceive other entities within a configurable range and cone. Structured similarly to LocomotionCapability. | |
| RMC-125 | Aggressive AI behavior | When the guard detects the caster via vision, pursue and strike. Includes vision strategies and fighting strategies. | RMC-123 RMC-124 |
| RMC-126 | Seed aggressive guard | Add a new guard entity (distinct from the patrol guard) inside the building, equipped with a club. | RMC-48 RMC-60 RMC-122 RMC-123 RMC-124 RMC-125 |
| RMC-127 | 🏁 Milestone 5 — The caster smashes the aggressive guard into the wall with a spell hard enough to kill him before he can attack | | RMC-126 |

## To Do — Other

| Key     | Title | Description | Blocked By |
|---------|-------|-------------|------------|
| RMC-100 | In-game entity creation and modification | Add the ability to create new entities and modify existing ones while the game is running — without restarting or editing seed data. This is a prerequisite for conveniently populating the world during development. | |
| RMC-101 | Save game state to database | Persist current world state (entities and their properties) to the database. | |
| RMC-14  | Design channeling and persistent effects | Written runes stay active while power is channeled. Define what "channeling" means mechanically — what keeps a spell alive, how it is terminated, and how the executor tracks ongoing effects. | |
| RMC-117 | Right-click move on canvas | Right-clicking a position on the canvas should move the caster to that location. | |
| RMC-143 | Facing angle rune | A rune that returns the facing direction of an entity as two Locations — the entity's center and the point on the unit circle matching its facing angle. Signature: (Set = OH) → [Location, Location]. Requires RMC-133. | RMC-133 |
| RMC-140 | EntityAccessService | Introduce a service that is the sole gateway through which spells read properties from entities. Mirrors EntitySetSelectService — spells do not reach into entity internals directly, they ask the service what the engine exposes. | |
| RMC-141 | Migrate magic system to a dedicated Magic assembly | Extract all magic evaluation and rune logic into a RunicMagic.Magic assembly. Magic must not access Entity, WorldModel, or any world internals directly — it speaks exclusively through the API surface World exposes (EntitySetSelectService, EntityAccessService, and any effect-execution interfaces). World owns the contract; Magic is a client of it. | RMC-139 RMC-140 |
| RMC-131 | Engine-layer collision response | When VUN(push) or other engine-layer motion detects a collision, the engine moves the obstacle aside by the minimum needed to complete the ordered motion, drawing additional spell power to do so. No damage is dealt to either party. Distinct from simulation-layer collision response (RMC-48). | RMC-130 |

## In Progress
| Key     | Title | Description | Remarks |
|---------|-------|-------------|---------|

## Ready For Review

| Key     | Title | Description |
|---------|-------|-------------|
| RMC-147 | Remove GetInAllInRayExceptSourceEntities | `GetInAllInRayExceptSourceEntities` is not a true engine primitive — it is a composition of `GetAllInRay` and `RAL`. Remove it from `EntitySetSelectService` and rework the `KAL` and `DAN` rune implementations to not rely on it. |

## Done

| Key     | Title |
|---------|-------|
| RMC-128 | Cone cast |
| RMC-146 | Promote GA(global) to EntitySet rune |
| RMC-139 | EntitySetSelectService |