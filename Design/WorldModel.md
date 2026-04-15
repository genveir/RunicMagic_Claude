# World Model

## Spatial Model

The world is a 2D coordinate space measured in centimeters. Every Entity has a position and dimensions, represented as an axis-aligned rectangle. There are no entities without spatial representation.

Spatial relationships between entities:

- **Containment** — entity A is contained by entity B if A's rectangle fits entirely within B's rectangle
- **Touch** — entities whose rectangles are adjacent or overlapping
- **Proximity** — the measurable distance between two entities, used by `DWOR(in range)`

## Entity Taxonomy

Everything in the world is an `Entity`. There is no type hierarchy in the magic system — the magic system operates purely on entities and their properties/capabilities.

Capabilities are first-class properties on an entity. The starting set:

- **HasLife** — the entity is alive; can be targeted by life-affecting magic, can die
- **HasAgency** — the entity can act autonomously
- **HasScope** — the entity defines its own scope: the set of entities the magic system considers reachable from it. What this means varies by entity — a cave's scope is its contents, a person's scope is what they are touching.
- **IsReservoir** — the entity can provide power. Exposes a single `Draw(amount)` method that returns how much power was actually given (which may be less than requested). The entity self-defines what draining means — a mana crystal depletes its reserves, a creature loses life. The engine never inspects a reservoir's internals; it just calls `Draw` and works with what it gets.

The implementation may use convenience classes (e.g. `Creature`) to stamp out entities with common property combinations, but these are purely an implementation concern. The magic system has no knowledge of them.
