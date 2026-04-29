# RunicMagic_Claude Todo

## To Do — Milestone 2

Next ticket number: RMC-91
Next bugfix number: BUG-6

| Key | Title | Description | Blocked By |
|-----|-------|-------------|------------|
| RMC-85 | Scale up power costs | Remove the `/1_000_000` divisor from motion cost formulas (VUN/VAR and `RotationCostCalculator`) so that 1 power unit = 1 gram-millimeter of work. This prevents small costs from flooring to zero via integer division. Add an overflow guard to `DrawPower`: if the requested cost exceeds `long.MaxValue / 2` (i.e. a cost so large that no realistic source stack could cover it), treat it as fully draining all available sources and return whatever was drawn. Update reservoir defaults and test cost values accordingly. | |
| RMC-86 | EventTracker — replace SpellResult with a unified change tracker | Rename `SpellResult` to `EventTracker` (or similar) and extend it to track two things: the existing spell events (text feedback) and a `HashSet<Entity>` of every entity touched during the operation. `MoveEntityService.Move` adds the moved entity; `PowerService.DrawPower` adds every entity drawn from. The tracker is already threaded through all call chains (`TryAdvance`, `DrawPower`, etc.) so no signature changes are needed beyond the type itself. `WorldModel.TickMotion` returns the tracker for the tick; the game loop reads its touched-entity set to push canvas deltas alongside any text. Track everything — don't filter by canvas-relevance at this layer. | RMC-85 |
| RMC-88 | Clean up TryAdvance/TryAdvanceInner split | `LinearMotionEffect` and `RotationMotionEffect` both have a public `TryAdvance` wrapper whose only job is to redirect `context.Result` before delegating to a private `TryAdvanceInner`. This is an artefact of how result redirection was bolted on. Collapse back to a single method — likely resolved naturally as part of the RMC-86 EventTracker overhaul. | RMC-86 |
| RMC-89 | CalcifiedEntitySet / CalcifiedLocation | Replace `FixedEntitySet` and `FixedLocation` in production with `CalcifiedEntitySet` and `CalcifiedLocation`. Each wraps an inner `IEntitySet` / `ILocation` expression, evaluates it on the first `Resolve()`/`Evaluate()` call, caches the result, and returns the cache on all subsequent calls. The parser inserts these wrappers by default when no live-tracking modifier is present. Effect runes become agnostic — they receive an `IEntitySet`/`ILocation` and pass it straight to the motion effect without inspecting or snapshotting it. Remove `FixedEntitySet` and `FixedLocation` from production code; restore them to the test project as simple test utilities. | |
| RMC-90 | Move linear motion cost calculation into LinearMotionEffect | VUN and VAR compute `totalCost` and `perTickCost` in the rune executor and pass the result in as a constructor parameter. CJIR and CJAR compute cost dynamically inside `RotationMotionEffect.TryAdvance` from the current entity state. Cost calculation should be the motion effect's responsibility in both cases — the rune executor should pass the entity set and distance/angle, not a pre-baked cost. This also correctly handles variable entity sets where the set of entities being moved isn't known at cast time. | RMC-89 |
| BUG-5 | Entity clicks break during canvas animation | While motion effects are animating, click events on entities are not registered — clicks in open space still work. Likely caused by the constant SSE-driven redraws recreating canvas elements or resetting event listener state between frames. Investigate whether hit targets are being torn down and rebuilt on each redraw, and fix so click handling on entities is stable regardless of redraw rate. | |
| RMC-77 | World AI system | Add an `AiCapability` to the entity model. World exposes `TickAi(deltaSeconds)` which iterates all entities with AI and calls their tick. | |
| RMC-80 | PatrolAi behavior | First concrete AI behavior: `PatrolAiCapability` — walks an entity back and forth along a list of waypoints at a configurable speed. Needs DB schema for waypoints and speed. | RMC-77 |
| RMC-83 | Movement service | Introduce a movement service that sits between any "I want to move" request (AI, future systems) and the actual position update. The service is the single place that knows about ongoing motion effects and gates or modifies autonomous movement accordingly. For now: an entity under an active motion effect cannot produce its own movement. | RMC-82 RMC-77 |
| RMC-79 | Guard NPC scenario | Add a guard entity to the test world with a patrol path along a visible corridor. The guard should be visually distinct. | RMC-80 RMC-78 RMC-83 |
| RMC-75 | 🏁 Milestone 3 — Have a guard walk by and get pushed | Implement a simple guard NPC that walks back and forth along a predefined path and push him away with a DAN-targeted spell. This will require the engine to have a concept of time. | RMC-79 RMC-82 |

## To Do — Other

| Key | Title | Description | Blocked By |
|-----|-------|-------------|------------|
| RMC-60 | Design small items | Define the world model for small items — portable objects a creature can carry (e.g. a mana gem in the caster's pocket). Covers how items are represented, how carrying/inventory works, and how items interact with spells and power sourcing. | |
| RMC-37 | Kill creatures when they run out of life | At any point during spell execution, living entities may run out of hitpoints. In this case the game should register that they're dead and remove their living and agency properties | |
| RMC-38 | Register entity destruction | It should be possible to destroy entities. When this happens, the game should register that the entity is destroyed | |
| RMC-39 | Stop spell execution on caster or executor death or destruction | When the caster or executor of a spell cease to be in an active state the spell should stop executing | RMC-37 RMC-38 |
| RMC-14 | Design channeling and persistent effects | Written runes stay active while power is channeled. Define what "channeling" means mechanically — what keeps a spell alive, how it is terminated, and how the executor tracks ongoing effects. | |
| RMC-48 | Formalize movement with collision | VUN currently teleports entities to their destination. Replace this with movement through space: the entity travels along the push vector and stops when it hits an entity (a wall, door, etc.) rather than passing through it. | |
| RMC-74 | Cone selection rune | A spatial selector that selects all entities within a cone projected from an origin entity in its pointing direction. Parametrised by angle (half-width) and range. | |
| BUG-4 | Up/Down arrow in terminal doesn't reprint prompt | When using the up or down arrow to redo earlier commands in the terminal, it cuts into the prompt | |
| RMC-84 | Partial view updates via SSE | Instead of posting the full world render on every game tick, post only the entities that changed during that tick. The client merges the delta into its current render state rather than replacing it wholesale. | RMC-78 |

## In Progress
| Key | Title | Description | Remarks |
|-----|-------|-------------|---------|

## Ready For Review

| Key | Title |
|-----|-------|
| RMC-82 | Motion effects queue |

## Done

| Key | Title |
|-----|-------|
| RMC-76 | Game clock |
| RMC-78 | SSE world push |
| RMC-81 | Queue spells for tick processing |