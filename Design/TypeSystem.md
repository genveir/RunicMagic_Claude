# Type System

## Types

### Statement
An executable world effect. Produced by effect runes (`VUN(push)`). Consumed by execution runes (`ZU(execute)`).
Statements are applied in the order their enclosing expressions resolve.

### Set
A set of entity references resolved against current world state. By default (calcified mode) the result is captured on first evaluation and reused; with `SA` the predicate is re-evaluated on every tick. See *Calcified vs Live Evaluation* below.

There is no singleton subtype; a Set with one member is still just a Set.

### Number
A non-negative integer.
Literals cover the primes up to 7 (plus zero) and powers of 14 at those same prime exponents, giving a sparse but wide range. Arithmetic runes (IR, MO, UIT, EID, DEID, MOST) allow any value to be composed from these building blocks.

### Location
A point in 2D space.

### ExecutableStatement
The return type of `ZU(execute)`. Marks the root of the expression tree; nothing can consume it.

## Subtyping and Coercion

None. Types are strict. A rune expecting a Number will not accept a Set; the offending rune and everything after it is ignored per the parse failure rule in MagicSystem.md.

## Default Arguments

A rune may declare a default expression for an optional trailing argument. If the next token in the stream does not produce the expected type, the parser substitutes the default and does not consume a token.

Defaults are resolved at parse time using the same recursive descent rules as explicit arguments, and they inherit the liveness mode in effect at the point of substitution. A default inside an `SA` context is live; a default inside a `YI` context (or the default calcified mode) is calcified.

Example: `VUN(push)`'s third argument defaults to `PAR(OH)`. If no Location-producing token follows the Number argument, the parser inserts `PAR OH` without consuming any input.

## Calcified vs Live Evaluation

All value types — Set, Location, Number — carry an implicit evaluation policy:

- **Calcified** (default): the expression is evaluated once, on first execution, and the result is cached for the duration of the effect. A motion effect evaluates a calcified set only on tick 1 and reuses the snapshot for all remaining ticks.
- **Live** (opt-in via `SA`): the expression is re-evaluated on every tick. The result may differ from tick to tick as world state changes.

Liveness is declared at parse time using the `SA(activate)` and `YI(calcify)` decorator runes. These are transparent to the runes that consume the expressions: a rune that receives a Set does not know or care whether it is calcified or live.

Liveness propagates upward through compound expressions. If any argument of a compound expression is live, the entire compound is live — it must re-evaluate to incorporate its live subtrees. `YI` can cut propagation for a specific subtree, freezing it even if it appears inside an `SA` context.

The distinction matters for selection cost: a calcified Set pays the full selection cost once. A live Set pays incrementally — only entities new to the set on a given tick incur the entity cost, and breadth is accumulated across all ticks rather than recharged each time.