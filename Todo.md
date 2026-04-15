# RunicMagic_Claude Todo

## To Do

Next ticket number: RMC-32

| Key | Title | Description | Blocked By |
|-----|-------|-------------|------------|
| RMC-31 | Add weight to entities | Making entities have weight makes the VUN rune's cost function more intuitive and fun | |
| RMC-12 | Design the parser | Turning a string of rune words into a typed expression tree. Direction: recursive descent where each rune implements its own Parse method, consuming its arguments from the remaining token stream — adding a rune stays self-contained. Parsing is the prerequisite for evaluation (type-checking) and distinct from execution (world application). Define how parse errors are surfaced. | RMC-6 |
| RMC-28 | Design the evaluator | Given a parsed expression tree, determine whether the spell is valid and what it costs. This is the type-checking phase: resolving rune output types against expected input types, surfacing type errors, computing power requirements. | RMC-6, RMC-12, RMC-27 |
| RMC-29 | Design the executor | Given a validated, costed spell, apply its effects against the world state. Defines what "executing" each rune means in terms of world mutations. Excludes power sourcing (RMC-15). | RMC-25, RMC-28 |
| RMC-30 | Wire up the spell casting pipeline | Build the SpellCastingService (or equivalent) in Controller that connects parser → evaluator → executor into a single callable entry point for the UI. Interface is derived from the implemented components rather than designed upfront. | RMC-12, RMC-28, RMC-29 |
| RMC-18 | 🏁 Walking skeleton — boot and cast anything | Milestone. Minimal world with a handful of hardcoded entities, minimal type system, minimal parser, minimal executor. Power source for the skeleton is caster health — find the caster entity and draw from its Reservoir directly. Goal: a user can launch the UI and successfully cast at least one spell that does something observable. Everything can be rough — correctness and completeness come later. | RMC-25, RMC-6, RMC-9, RMC-10, RMC-12, RMC-28, RMC-29, RMC-30 |
| RMC-13 | Design spell failure modes | RunicMagic.md has "[the spell fails]" as a placeholder. Beyond the executor-disintegrates rule for insufficient evaluation power, define what failure looks like for invalid spells, type mismatches, missing targets, etc. | |
| RMC-7 | Identify missing runes | HOR returns a Property but there is no comparison or conditional branching rune. Audit the full rune set for gaps needed to write meaningful spells. | RMC-6 |
| RMC-14 | Design channeling and persistent effects | Written runes stay active while power is channeled. Define what "channeling" means mechanically — what keeps a spell alive, how it is terminated, and how the executor tracks ongoing effects. | RMC-25 |
| RMC-15 | Design power sourcing implementation | The sourcing rules (explicit > executor-scope > caster-scope > local-scope, with preference ordering) are complex enough to deserve their own design and implementation ticket. | RMC-23, RMC-25, RMC-6 |
| RMC-16 | Design inscribed spells on objects | Spells inscribed on objects have different executor/caster semantics. Define how the world model represents inscribed spells, how they are activated, and how the evaluator handles the executor being an object rather than a creature. | RMC-25 |

## In Progress
| Key | Title | Description | Remarks |
|-----|-------|-------------|---------|

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
| RMC-26 | Build the SVG canvas panel |
| RMC-5 | Design the interaction model |
| RMC-25 | Implement the world model |
| RMC-6 | Formalize the type system |
| RMC-27 | Define the initial rune set |