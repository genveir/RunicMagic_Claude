# Type System

## Types

### Statement
An executable world effect. Produced by effect runes (`VUN(push)`). Consumed by execution runes (`ZU(execute)`).
Statements are applied in the order their enclosing expressions resolve.

### Set
A set of entity references, evaluated as a predicate against current world state at the point of execution. 

There is no eager capture — a Set referenced twice in a spell reflects the world state at each point of evaluation. 

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

Defaults are resolved at parse time using the same recursive descent rules as explicit arguments.

Example: `VUN(push)`'s third argument defaults to `PAR A(location of me)`. If no Location-producing token follows the Number argument, the parser inserts `PAR A` without consuming any input.