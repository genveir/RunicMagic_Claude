# Fiction — World Model

## The World as Construct

The game world is not a natural environment — it is a magical construct. Everything in it was shaped, and continues to be governed, by rune magic. The physics, the entities, the terrain: all of it exists because the engine says it does.

Rune magic is a functional programming language. A spell is a script, and casting it runs that script against the world. The engine that executes spells is the same engine that simulates the world. Magic is not a force operating *within* the simulation — it is the simulation's operator.

---

## Two Layers

This produces a clean two-layer model that drives architectural decisions throughout the codebase.

### Engine layer (magic)

Spell execution operates above the simulation. When VUN pushes an entity, the engine is directly rewriting world state — it is not applying a force that the simulation then resolves. Engine-layer movement bypasses friction and gravity — those would just interfere with an explicit instruction.

Collisions are handled, but differently from the simulation layer. If an entity being moved by VUN has another entity in its path, the engine moves that obstacle out of the way by exactly the minimum needed to complete the ordered motion. The spell draws power to do so. Neither party takes damage.

### Simulation layer (physics)

Entities that move under their own agency — a guard walking a patrol path, a stone in flight — are moving *within* the simulation. This movement is subject to physics: friction, gravity, collision, and whatever other rules govern the simulated world. These are not instructions to the engine; they are behaviours of entities participating in a modelled environment.

Simulation-layer collisions behave as expected. A person walking into a wall will displace it slightly (which it then recovers from) and bounce off. Both parties take damage, more to the moving entity.

---

## Design Consequence

Engine-driven movement and simulation-driven movement are categorically different things and must be represented as separate systems — even when they share low-level infrastructure like collision detection. The collision *detection* may be the same; the collision *response* is entirely different.

A guard walking is locomotion. VUN pushing a guard is the engine overriding that guard's position. While the engine is overriding, the guard's locomotion is suspended — not because locomotion "lost," but because the engine layer does not negotiate with the simulation layer.
