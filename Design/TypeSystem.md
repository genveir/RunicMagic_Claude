# Type System

## Types

### Statement
An executable world effect. Produced by effect runes (VUN). Consumed by ZU.
Statements are applied in the order their enclosing expressions resolve.

### Group
A set of entity references produced by selection runes (BUZD). Captures the entities
present in scope at evaluation time. If an entity is destroyed mid-execution, behaviour
for already-captured references is left to RMC-29.

### Scope
A pool of entities from which a Group can be selected. Not directly usable as an effect
target. Resolves by calling the entity's `Scope` delegate at evaluation time.

### Number
A non-negative integer. Currently only expressible as powers of 14 via HET and FOTIR.
Arithmetic is intentionally omitted for now — the available range (1, 14, 196, 2744, …)
is sufficient to scale effects from trivial to lethal.

### Location
A point in 2D space (x, y in centimetres). Produced by PAR from an Entity. Resolves to
the centre point of the entity's bounding rectangle at evaluation time.

### Entity
A reference to a specific world entity. Currently only OH produces one directly.

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

Example: VUN's third argument defaults to `PAR(OH)`. If no Location-producing token
follows the Number argument, the parser inserts `PAR(OH)` without consuming any input.
