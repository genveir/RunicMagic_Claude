# Type System

## Types

### Statement
An executable world effect. Produced by effect runes (VUN). Consumed by ZU.
Statements are applied in the order their enclosing expressions resolve.

### Set
A set of entity references, evaluated as a predicate against current world state at the
point of execution. There is no eager capture — a Set referenced twice in a spell
reflects the world state at each point of evaluation. A set produced by `A` or `OH` is
a singleton, but there is no singleton subtype; it is still just a Set.

`LA` maps a Set to the union of its members' scopes. Other set operations (filter, union,
intersection, difference) are natural future rune primitives.

### Number
A non-negative integer. Currently only expressible as powers of 14 via HET and FOTIR.
Arithmetic is intentionally omitted for now — the available range (1, 14, 196, 2744, …)
is sufficient to scale effects from trivial to lethal.

### Location
A point in 2D space (x, y in centimetres). Produced by PAR from a Set. Resolves to the
centroid of the member entities' bounding rectangles at evaluation time.

### ExecutableStatement
The return type of ZU. Marks the root of the expression tree; nothing can consume it.

## Subtyping and Coercion

None. Types are strict. A rune expecting a Number will not accept a Group; the offending
rune and everything after it is ignored per the parse failure rule in MagicSystem.md.

## Default Arguments

A rune may declare a default expression for an optional trailing argument. If the next
token in the stream does not produce the expected type, the parser substitutes the default
and does not consume a token. Defaults are resolved at parse time using the same recursive
descent rules as explicit arguments.

Example: VUN's third argument defaults to `PAR(A)`. If no Location-producing token
follows the Number argument, the parser inserts `PAR(A)` without consuming any input.
