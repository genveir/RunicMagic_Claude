# RunicMagic_Claude Todo

## To Do

Next ticket number: RMC-27

| Key | Title | Description | Blocked By |
|-----|-------|-------------|------------|
| RMC-26 | Build the SVG canvas panel | Add an SVG canvas above the terminal that renders world entities as labelled rectangles. Wire it to the `onRenderingDataAvailable` and `onTickComplete` callbacks on `IPlayerViewInterface`. Canvas is responsible for mapping world coordinates to screen coordinates. Entities are styled by capability flags (`HasLife`, `HasAgency`, etc.). | RMC-10 |
| RMC-25 | Implement the world model | Implement entity storage, querying, and identity in code. How are entities stored at runtime? How are they queried by type, scope, or property? How is identity represented? | RMC-20, RMC-22, RMC-23, RMC-24 |
| RMC-6 | Formalize the type system | Define all types used in the rune system (Statement, ExecutedStatement, PowerSource, PowerReservoir, Number, Property, Position, Group, Area, Scope, Action, boolean, object, etc.), their relationships, and how subtyping/coercion works. | |
| RMC-5 | Design the interaction model | How does a user interact with the evaluator? Options: REPL (type runes, see effects), scripted world with simulated entities, something else? | |
| RMC-12 | Design the parser | Turning a string of rune words into a typed expression tree is a distinct concern from evaluation. Direction: recursive descent where each rune implements its own Parse method, consuming its arguments from the remaining token stream. This means adding a rune is self-contained. Define how type errors are surfaced. | RMC-6 |
| RMC-8 | Design the evaluator and executor | Define how spell expressions are evaluated for cost and executed against the world state. Excludes parsing (RMC-12) and power sourcing (RMC-15). | RMC-25, RMC-6, RMC-12 |
| RMC-18 | 🏁 Walking skeleton — boot and cast anything | Milestone. Minimal world with a handful of hardcoded entities, minimal type system, minimal parser, minimal executor. Goal: a user can launch the UI and successfully cast at least one spell that does something observable. Everything can be rough — correctness and completeness come later. | RMC-25, RMC-6, RMC-8, RMC-9, RMC-10, RMC-12 |
| RMC-17 | Re-evaluate runes against the formalized systems | Once the type system and world model are settled, review all runes in Runes.md for consistency, gaps, and anything that no longer makes sense. Update the design document accordingly. | RMC-20, RMC-22, RMC-6 |
| RMC-13 | Design spell failure modes | RunicMagic.md has "[the spell fails]" as a placeholder. Beyond the executor-disintegrates rule for insufficient evaluation power, define what failure looks like for invalid spells, type mismatches, missing targets, etc. | |
| RMC-7 | Identify missing runes | HOR returns a Property but there is no comparison or conditional branching rune. Audit the full rune set for gaps needed to write meaningful spells. | RMC-6 |
| RMC-14 | Design channeling and persistent effects | Written runes stay active while power is channeled. Define what "channeling" means mechanically — what keeps a spell alive, how it is terminated, and how the executor tracks ongoing effects. | RMC-25 |
| RMC-15 | Design power sourcing implementation | The sourcing rules (explicit > executor-scope > caster-scope > local-scope > global-scope, with preference ordering) are complex enough to deserve their own design and implementation ticket. | RMC-23, RMC-25, RMC-6 |
| RMC-16 | Design inscribed spells on objects | Spells inscribed on objects have different executor/caster semantics. Define how the world model represents inscribed spells, how they are activated, and how the evaluator handles the executor being an object rather than a creature. | RMC-25 |

## In Progress

## Ready For Review

| Key | Title |
|-----|-------|

## Done

| Key | Title |
|-----|-------|
| RMC-1 | Convert the source documents to a readable format |
| RMC-20 | Define entity taxonomy |
| RMC-19 | How does magic work? |
| RMC-22 | Define the scope and spatial model |
| RMC-23 | Define power sources in the world |
| RMC-24 | Define how magic references entities |
| RMC-9 | Decide on the UI layout |
| RMC-10 | Build the UI |