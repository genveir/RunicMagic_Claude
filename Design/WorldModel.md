# World Model

## Spatial Model

The world is a 2D coordinate space measured in millimeters. Every Entity has a position and dimensions, represented as an axis-aligned rectangle. There are no entities without spatial representation.

Spatial relationships between entities:

- **Containment** — entity A is contained by entity B if A's rectangle fits entirely within B's rectangle
- **Touch** — entities whose rectangles are adjacent or overlapping
- **Proximity** — the measurable distance between two entities

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
| `HasAgency` | Property | boolean | `false` = no agency |
| `IsTranslucent` | Property | boolean | `false` = opaque; used by ray-cast runes |
| `Life` | Property | complex | `LifeCapability`: `MaxHitPoints` + `CurrentHitPoints`; null = not alive |
| `Charge` | Property | complex | `ChargeCapability`: `MaxCharge` + `CurrentCharge`; null = uncharged |
| `PointingDirection` | Transient | scalar | The direction the entity is consciously aiming; null = not pointing |
| `Scope` | Derived | delegate | Returns the set of entities reachable from this entity |
| `Reservoir` | Derived | delegate | Draws power; closes over whichever property holds its state |

A complex property's null object is its own "absent" marker — no separate boolean needed. A boolean property uses `false` as its absent marker.

The implementation may use convenience classes (e.g. `Creature`) to stamp out entities with common data combinations, but these are purely an implementation concern. The magic system has no knowledge of them.

## Implementation Model

### Entity structure

`Entity` is a mutable class. Its data maps to the categories above:

**Attributes** (always present):
- `EntityId Id`
- `string Label`
- `int Weight`
- Spatial bounds: `int X, Y, Width, Height`

**Properties** (optional, persisted):
- `bool HasAgency` — false = absent
- `bool IsTranslucent` — false = absent
- `LifeCapability? Life` — null = absent; holds `MaxHitPoints` + `CurrentHitPoints`
- `ChargeCapability? Charge` — null = absent; holds `MaxCharge` + `CurrentCharge`

**Transient** (session-only):
- `Direction? PointingDirection` — null = not pointing

**Derived** (wired at load, no persistence):
- `Func<Entity[]>? Scope` — computed on call; closes over world state
- `Func<int, ReservoirDraw>? Reservoir` — takes amount requested, returns draw result; closes over whichever property holds its state

### Derived delegates

`Reservoir` and `Scope` are delegates rather than interface implementations. Their state lives in the property objects (`LifeCapability`, `ChargeCapability`) that the closures capture — not inside the delegate itself. This keeps state inspectable and persistable.

Examples:
- A creature's `Reservoir` closes over its `LifeCapability` and draws from `CurrentHitPoints`.
- A mana source's `Reservoir` closes over its `ChargeCapability` and draws from `CurrentCharge`.
- A creature's `Scope` closes over the `WorldModel` and returns entities whose bounds touch the creature's bounds.

Properties are orthogonal. An entity that has both `Life` and `Charge` could wire a `Reservoir` that draws from either. The engine never inspects which.

### EntityType and persistence

Delegates cannot be stored in the database. Each entity has an `EntityType` discriminator (stored as a foreign key) that identifies which delegates to wire at load time. `EntityFactory` reconstructs the correct closures for each type when the world is loaded into memory.

The world is loaded in full at startup into a `Dictionary<EntityId, Entity>`. There is no on-demand querying — all reads are in-memory scans.

### Spatial queries

`WorldModel` exposes three spatial queries, all implemented as linear scans:

- `GetEntitiesInArea(Rectangle)` — entities whose bounds overlap the given area (strict intersection, excludes edge-adjacent)
- `GetTouchingEntities(Entity)` — entities whose bounds overlap or share an edge with the given entity
- `GetContainedEntities(Entity)` — entities whose bounds fit entirely within the given entity's bounds

`Rectangle` is a custom `readonly record struct` (integer millimetres) with no dependency on `System.Drawing`.
