# World Model

## Spatial Model

The world is a 2D coordinate space measured in millimeters. Every Entity has a position, dimensions, and an orientation angle, represented as an oriented (rotatable) rectangle. There are no entities without spatial representation.

Spatial relationships between entities:

- **Containment** — entity A is contained by entity B if all of A's corners lie within B's rectangle
- **Touch** — entities whose rectangles intersect or are adjacent
- **Proximity** — the measurable surface-to-surface distance between two entities

## Entity Taxonomy

Everything in the world is an `Entity`. There is no type hierarchy in the magic system — the magic system operates purely on entities and their data.

### Entity data categories

| Category | Meaning | Absent expressed as |
|---|---|---|
| **Attribute** | Mandatory, always present, persisted | N/A |
| **Property** | Optional, persisted | `false` (boolean), `null` (scalar or complex) |
| **Transient** | Session-only, not persisted | `null` |
| **Derived** | Computed from other state; no own storage | N/A |

Current entity data:

| Name | Category | Type | Notes |
|---|---|---|---|
| `Id`, `Label` | Attribute | — | Out-of-world concerns: persistence identity and display |
| `Weight`, bounds | Attribute | scalar | |
| `DragCoefficient` | Attribute | scalar | Aerodynamic resistance coefficient; used with projected cross-sectional area to compute drag force each physics tick |
| `HasAgency` | Property | boolean | `false` = no agency |
| `IsTranslucent` | Property | boolean | `false` = opaque; used by ray-cast runes |
| `Life` | Property | complex | `LifeCapability`: `MaxHitPoints` + `CurrentHitPoints`; null = not alive |
| `Charge` | Property | complex | `ChargeCapability`: `MaxCharge` + `CurrentCharge`; null = uncharged |
| `StructuralIntegrity` | Attribute | complex | `StructuralIntegrityCapability`: `MaxIntegrity` + `CurrentIntegrity`; always present. Damage to structural integrity also caps `Life.CurrentHitPoints` to the new `CurrentIntegrity` — the reverse does not apply. |
| `AI` | Property | complex | `AICapability`: collection of AI behaviours; empty = no AI |
| `Locomotion` | Property | complex | `LocomotionCapability`: movement parameters; null = cannot move under own agency |
| `PointingDirection` | Transient | scalar | The direction the entity is consciously aiming; null = not pointing |
| `IndicateTarget` | Transient | complex | The entity the caster is consciously indicating, with optional approach direction; null = not indicating |
| `RawInscriptions` | Transient | `string[]` | Raw inscription texts loaded from the `Inscription` table; empty = none |
| `ParsedInscriptions` | Transient | `IStatement[]` | Spells inscribed on this entity, pre-parsed at load time from `RawInscriptions`; empty = none. Any inscription whose rune text fails to parse is silently dropped. Any entity type can have inscriptions. |
| `IsUnderEngineMotion` | Transient | boolean | `false` = not under engine motion; set by the game loop while an engine motion effect is active for this entity; `PhysicsService` skips entities where this is true |
| `Velocity` | Transient | `VelocityVector?` | Current velocity in mm/tick; null = stationary. Integrated by `PhysicsService` each tick. |
| `PendingImpulses` | Transient | `List<ForceVector>` | Force vectors (g·mm/tick²) queued for this tick; written by AI behaviours, locomotion, and spells; consumed and cleared by `PhysicsService` each tick |
| `Scope` | Derived | delegate | Returns the set of entities reachable from this entity |
| `Reservoir` | Derived | `ReservoirCapability?` | Exposes power query and draw/fill operations; closes over whichever property holds its state; null = no power |

A complex property's null object is its own "absent" marker — no separate boolean needed. A boolean property uses `false` as its absent marker.

The implementation may use convenience classes (e.g. `Creature`) to stamp out entities with common data combinations, but these are purely an implementation concern. The magic system has no knowledge of them.

## Implementation Model

### Entity structure

`Entity` is a mutable class. Its data maps to the categories above:

**Attributes** (always present):
- `EntityId Id`
- `string Label`
- `long Weight`
- Position: `Location Location` (holds `double X, Y`)
- Dimensions: `long Width, Height`
- `double Angle` — orientation in radians; 0 = axis-aligned
- `double DragCoefficient` — aerodynamic resistance; 0 = no drag

**Properties** (optional, persisted):
- `bool HasAgency` — false = absent
- `bool IsTranslucent` — false = absent
- `LifeCapability? Life` — null = absent; holds `MaxHitPoints` + `CurrentHitPoints`
- `ChargeCapability? Charge` — null = absent; holds `MaxCharge` + `CurrentCharge`
- `StructuralIntegrityCapability StructuralIntegrity` — always present; stored as columns on `Entities`; holds `MaxIntegrity` + `CurrentIntegrity`
- `AICapability AI` — always present as an object; empty = no AI behaviours
- `LocomotionCapability? Locomotion` — null = cannot move under own agency

**Transient** (session-only):
- `Direction? PointingDirection` — null = not pointing
- `IndicateTarget? IndicateTarget` — entity the caster consciously indicates, with optional approach direction; null = not indicating
- `string[] RawInscriptions` — raw inscription texts loaded at world load; empty = none
- `IStatement[] ParsedInscriptions` — spells inscribed on this entity; pre-parsed from `RawInscriptions` at world load; empty = none. Any inscription whose rune text fails to parse is silently dropped.
- `bool IsUnderEngineMotion` — set by the game loop while an engine motion effect is active for this entity; `PhysicsService` skips this entity while the flag is set
- `VelocityVector? Velocity` — current velocity in mm/tick; null = stationary
- `List<ForceVector> PendingImpulses` — force vectors queued this tick; cleared by `PhysicsService` after integration

**Derived** (wired at load, no persistence):
- `Func<Entity[]>? Scope` — computed on call; closes over world state
- `ReservoirCapability? Reservoir` — exposes `Max`, `Current`, `Draw`, and `Fill` delegates; closes over whichever property holds its state; null = no power

### Derived delegates

`Reservoir` and `Scope` are wired at load time. Their state lives in the property objects (`LifeCapability`, `ChargeCapability`) that the closures capture — not inside the delegate objects themselves. This keeps state inspectable and persistable.

`ReservoirCapability` bundles four delegates:
- `Func<long> Max` — returns the entity's maximum power capacity
- `Func<long> Current` — returns the entity's current power level
- `Func<long, ReservoirDraw> Draw` — draws up to N power; returns how much was drawn and whether the reservoir is now drained
- `Func<long, ReservoirFill> Fill` — fills up to N power; returns how much was filled and whether the reservoir is now full

Examples:
- A creature's `Reservoir` closes over its `LifeCapability` and draws from `CurrentHitPoints`.
- A mana source's `Reservoir` closes over its `ChargeCapability` and draws from `CurrentCharge`.
- A creature's `Scope` closes over the `WorldModel` and returns entities whose bounds touch the creature's bounds.

Properties are orthogonal. An entity that has both `Life` and `Charge` could wire a `Reservoir` that draws from either. The engine never inspects which.

### EntityType and persistence

Delegates cannot be stored in the database. Each entity has an `EntityType` discriminator (stored as a foreign key) that identifies which delegates to wire at load time. `EntityFactory` reconstructs the correct closures for each type when the world is loaded into memory.

The world is loaded in full at startup into a `Dictionary<EntityId, Entity>`. There is no on-demand querying — all reads are in-memory scans.

### Spatial queries

`WorldModel` exposes spatial queries, all implemented as linear scans:

- `GetAll()` — all entities in the world
- `Find(EntityId)` — single entity by identity; returns null if not found
- `GetEntitiesAtPoint(Location)` — entities whose bounds contain the given point
- `GetEntitiesWithinDistance(Entity, double)` — entities whose bounds are within the given surface-to-surface distance of the source entity's bounds; excludes the source itself; used to compute scope
- `GetContainedEntities(Entity)` — entities whose bounds fit entirely within the container's bounds; excludes the container itself

### Rectangle

`Rectangle` is a `readonly record struct` with no dependency on `System.Drawing`. It represents an oriented rectangle: `Location` is the centre point, `Width` and `Height` are the full extents (as `double`), and `Angle` is the orientation in radians. All spatial operations (containment, intersection, ray-cast, distance) account for rotation. `GetProjectedWidth(Direction)` returns the silhouette width perpendicular to a direction of motion; used by `PhysicsService` to compute the cross-sectional area exposed to drag.

## Motion

Motion in the world has two orthogonal axes. See `Design/Fiction.md` for the design rationale behind the engine/simulated split.

### Engine vs Simulated

**Engine motion** is the magic system directly rewriting an entity's position. It does not negotiate with physics. The effect runes `VUN(push)`, `VAR(pull)`, `CJIR(rotate clockwise)`, and `CJAR(rotate counterclockwise)` produce engine motion effects that advance over `Constants.DefaultEffectLength` ticks.

**Simulated motion** is an entity moving under its own agency — a creature walking, a projectile in flight. It participates in the physics simulation and is subject to drag and other environmental forces. `PhysicsService` drives this layer.

Each tick, AI behaviours, locomotion, and spells write `ForceVector` impulses to `Entity.PendingImpulses`. `PhysicsService` then integrates all pending impulses into velocity, applies drag (using `DragCoefficient` and the entity's projected cross-sectional area), and updates position via `MoveEntityService`.

While an entity is under engine motion, its simulated physics are suspended. The bridge between the two layers is the transient flag `Entity.IsUnderEngineMotion`. The game loop sets this flag on any entity that appears in an active engine motion effect's entity set; `PhysicsService` skips and clears impulses for any entity where it is set.

### Move vs Teleport

**Move** carries an entity from its current position to a destination by traversing the space in between. The entity occupies every intermediate point along the path. Both engine motion effects and `PhysicsService` use move semantics — the difference is who is driving and whether physics applies.

**Teleport** is an instantaneous position change. The entity does not pass through intermediate space and does not interact with anything along the way. `TeleportEntityService` provides this operation and is used for placement and similar out-of-simulation repositioning.
