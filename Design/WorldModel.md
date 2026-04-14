# World Model

## Entity Taxonomy

Everything in the world is an `Entity`. There is no type hierarchy in the magic system — the magic system operates purely on entities and their properties/capabilities.

Capabilities are first-class properties on an entity. The starting set:

- **HasLife** — the entity is alive; can be targeted by life-affecting magic, can die
- **HasAgency** — the entity can act autonomously
- **HasScope** — the entity defines what "in scope of this entity" means (see Scope and Spatial Model)
- **IsReservoir** — the entity can store and provide power

The implementation may use convenience classes (e.g. `Creature`) to stamp out entities with common property combinations, but these are purely an implementation concern. The magic system has no knowledge of them.
