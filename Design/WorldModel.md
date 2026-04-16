# World Model

## Spatial Model

The world is a 2D coordinate space measured in centimeters. Every Entity has a position and dimensions, represented as an axis-aligned rectangle. There are no entities without spatial representation.

Spatial relationships between entities:

- **Containment** — entity A is contained by entity B if A's rectangle fits entirely within B's rectangle
- **Touch** — entities whose rectangles are adjacent or overlapping
- **Proximity** — the measurable distance between two entities

## Entity Taxonomy

Everything in the world is an `Entity`. There is no type hierarchy in the magic system — the magic system operates purely on entities and their properties/capabilities.

Capabilities are first-class properties on an entity. The starting set:

- **HasLife** — the entity is alive; can be targeted by life-affecting magic, can die
- **HasAgency** — the entity can act autonomously
- **HasScope** — the entity defines its own scope: the set of entities the magic system considers reachable from it. What this means varies by entity — a cave's scope is its contents, a person's scope is what they are touching.
- **IsReservoir** — the entity can provide power. Exposes a single `Draw(amount)` method that returns how much power was actually given (which may be less than requested). The entity self-defines what draining means — a mana crystal depletes its reserves, a creature loses life. The engine never inspects a reservoir's internals; it just calls `Draw` and works with what it gets.

The implementation may use convenience classes (e.g. `Creature`) to stamp out entities with common property combinations, but these are purely an implementation concern. The magic system has no knowledge of them.

## Implementation Model

### Entity structure

`Entity` is a mutable class with typed nullable capability properties:

- `LifeCapability? Life` — `MaxHitPoints` + `CurrentHitPoints`
- `ChargeCapability? Charge` — `MaxCharge` + `CurrentCharge`
- `bool HasAgency`
- `Func<Entity[]>? Scope` — computed on call; closes over the world state
- `Func<int, int>? Reservoir` — takes amount requested, returns amount drawn; closes over whichever capability holds its state

A null property means the entity does not have that capability.

### Capability delegates

`Reservoir` and `Scope` are delegates rather than interface implementations. Their state lives in the capability objects (`LifeCapability`, `ChargeCapability`) that the closures capture — not inside the delegate itself. This keeps state inspectable and persistable.

Examples:
- A creature's `Reservoir` closes over its `LifeCapability` and draws from `CurrentHitPoints`.
- A mana source's `Reservoir` closes over its `ChargeCapability` and draws from `CurrentCharge`.
- A creature's `Scope` closes over the `WorldModel` and returns entities whose bounds touch the creature's bounds.

Capabilities are orthogonal. An entity that has both `Life` and `Charge` could wire a `Reservoir` that draws from either. The engine never inspects which.

### EntityType and persistence

Delegates cannot be stored in the database. Each entity has an `EntityType` discriminator (stored as a foreign key) that identifies which delegates to wire at load time. `EntityFactory` reconstructs the correct closures for each type when the world is loaded into memory.

The world is loaded in full at startup into a `Dictionary<EntityId, Entity>`. There is no on-demand querying — all reads are in-memory scans.

### Spatial queries

`WorldModel` exposes three spatial queries, all implemented as linear scans:

- `GetEntitiesInArea(Rectangle)` — entities whose bounds overlap the given area (strict intersection, excludes edge-adjacent)
- `GetTouchingEntities(Entity)` — entities whose bounds overlap or share an edge with the given entity
- `GetContainedEntities(Entity)` — entities whose bounds fit entirely within the given entity's bounds

`Rectangle` is a custom `readonly record struct` (integer centimetres) with no dependency on `System.Drawing`.
