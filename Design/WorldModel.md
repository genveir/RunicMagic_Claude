# World Model

## Spatial Model

The world is a 2D coordinate space measured in millimeters. Every Entity has a position, dimensions, and an orientation angle, represented as an oriented (rotatable) rectangle. There are no entities without spatial representation.

Spatial relationships between entities:

- **Containment** — entity A is contained by entity B if all of A's corners lie within B's rectangle
- **Touch** — entities whose rectangles intersect or are adjacent
- **Proximity** — the measurable surface-to-surface distance between two entities

## Entity Taxonomy

Everything in the world is an `Entity`. There is no type hierarchy in the magic system — the magic system operates purely on entities and their data.

Entity data falls into four categories:

| Category | Meaning | Absent expressed as |
|---|---|---|
| **Attribute** | Mandatory, always present, persisted | N/A |
| **Property** | Optional, persisted | `false` (boolean), `null` (scalar or complex) |
| **Transient** | Session-only, not persisted | `null` |
| **Derived** | Computed from other state; no own storage | N/A |

A complex property's null object is its own "absent" marker — no separate boolean needed. A boolean property uses `false` as its absent marker.

## Capabilities

A Capability is an optional domain object that encapsulates both the state and the behaviour of something an entity can *be or do*. It is not a flag — it is the ability itself. A dog has a `LocomotionCapability` not because it is tagged as "can move," but because it holds the movement parameters and knows how to apply forces, brake, and predict stopping distance. The entity doesn't know how to move; its `LocomotionCapability` does. Gaining a Capability means gaining the ability in a concrete sense; losing it means losing it entirely.

Most Capabilities are modeled as nullable properties: null means absent, a non-null object means present and active. One exception is the AI Capability, which is always present as an object but may contain no behaviours. The intent is the same — most entities have no AI — but the implementation keeps a non-null empty collection because ticking all entities over an empty list is cheaper than null-checking every entity on every tick.

## Notable design decisions in the data model

- `StructuralIntegrity` is an Attribute (always present, never null). Damage to structural integrity also caps `CurrentHitPoints` — the reverse does not apply.
- `AI` is always present as an object; empty means no behaviours. This avoids null checks throughout the AI system.
- Any entity type can carry inscriptions. The magic system does not restrict spells to creatures or specific entity kinds. Inscriptions that fail to parse at load time are silently dropped.
- `IsUnderEngineMotion` is transient and set by the game loop, not by the physics or magic layers — it is the bridge between engine and simulated motion (see below).

## AI Behaviours and Strategies

AI in the world is organised in three layers.

**Capabilities** are the bottom layer. They translate intent into concrete world actions — applying forces, predicting stopping distances, drawing power. A capability knows nothing about goals or tactics; it only knows how to act on the physics of the entity it belongs to.

**Strategies** are the middle layer. A strategy is a stateless, reusable algorithm that expresses a specific navigational or tactical task — "orient toward this direction," "navigate to this point." Strategies compose capability primitives to achieve a single well-defined outcome, and can be composed with each other. They carry no state of their own.

**Behaviours** are the top layer. A behaviour is stateful and goal-driven — it owns the state machine, the waypoints, the decision logic. It delegates the actual movement decisions to strategies, and through them to the capability. A behaviour knows *what* the entity is trying to do; the strategy knows *how* to make progress toward a specific sub-goal; the capability knows *how to act* on the world.

The AI Capability is the executor that runs an entity's behaviours each tick. Its design anticipates a split into a true interface: a noop implementation for entities with no AI (the common case), and a behavioral implementation for entities with decision logic. This avoids per-tick null checks across the entire entity population.

## Reservoir and Scope

`Reservoir` and `Scope` are derived — they carry no state of their own. They are delegates wired at load time that close over the entity's actual capability objects (`LifeCapability`, `ChargeCapability`, etc.). This keeps state inspectable and persistable while letting the magic system treat all power sources uniformly.

Properties are orthogonal. An entity with both `Life` and `Charge` could wire a `Reservoir` that draws from either. The engine never inspects which.

Because delegates cannot be persisted, each entity has an `EntityType` discriminator stored in the database. `EntityFactory` uses this to wire the correct closures when the world is loaded.

## Spatial Queries

`WorldModel` exposes spatial queries implemented as linear scans. The world is loaded in full at startup — there is no on-demand querying.

## Motion

Motion has two orthogonal axes. See `Design/Fiction.md` for the design rationale behind the engine/simulated split.

### Engine vs Simulated

**Engine motion** is the magic system directly rewriting an entity's position. It does not negotiate with physics. The effect runes `VUN(push)`, `VAR(pull)`, `CJIR(rotate clockwise)`, and `CJAR(rotate counterclockwise)` produce engine motion effects.

**Simulated motion** is an entity moving under its own agency — a creature walking, a projectile in flight. It participates in the physics simulation and is subject to drag and other environmental forces.

While an entity is under engine motion, its simulated physics are suspended.

### Move vs Teleport

**Move** carries an entity from its current position to a destination by traversing the space in between. The entity occupies every intermediate point along the path. Both engine motion and physics use move semantics.

**Teleport** is an instantaneous position change. The entity does not pass through intermediate space. Used for placement and out-of-simulation repositioning.
