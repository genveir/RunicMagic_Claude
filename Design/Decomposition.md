# Filter Runes — Property Type Redesign

This document describes a planned redesign of the property-based selector and filter runes. It will be folded into RuneSet.md and TypeSystem.md when implemented (RMC-132).

## Motivation

The current rune set encodes property roots and filter directions as fused tokens (`ZYHE`, `FUSE`, `HORIL`, etc.). This works but prevents composition: adding a new property root requires adding a full family of runes. Splitting roots from filter directions makes the system compositional and exposes the full expressive range of the language — including combinations that are valid but only survivable by casters with godlike power pools.

## New Type: Property

A `Property` is a new rune type representing a measurable attribute of an entity. It is produced by property root runes and consumed by selector and filter runes.

Properties are intrinsic or relational:

- **Intrinsic** — the value belongs to the entity alone (`ZY`, `FU`)
- **Relational** — the value is defined relative to something else (`HOR`) — the reference is constitutive, not a convenience parameter

## Property Roots

| Rune | Meaning | Signature |
|------|---------|-----------|
| `ZY` | weight | `() → Property` |
| `FU` | power | `() → Property` |
| `HOR` | distance from origin | `(Set origin = OH) → Property` |

`HOR` takes an origin `Set` because distance is not an intrinsic property of an entity — it only exists as a relation between two things. `HOR OH` and `HOR A` are genuinely different properties.

## Selector Rune

| Rune | Meaning | Signature |
|------|---------|-----------|
| `O` | less than | `(Property, Number threshold, Set = GA) → Set` |

`O` is a world-level selector: it reaches into the engine and selects all entities in the input `Set` whose property value is strictly less than `threshold`. The default input set is `GA` (all entities), making it an engine-primitive operation.

For `O HOR`, the engine uses its native spatial selection system rather than iterating the full entity list — this is the only exception to the general rule that `O` over `GA` iterates the world. For `O ZY` and `O FU` over `GA`, the full world is iterated and the breadth cost is proportional to the world population. This is suicidal for mortal casters but routine for the gods who built the world, who operate with near-infinite power pools.

## Filter Runes

Filters operate on an already-bounded input `Set`. They do not default to `GA` — the caller is responsible for scoping the candidate pool first.

| Rune | Meaning | Signature |
|------|---------|-----------|
| `IL` | range filter | `(Property, Number lower, Number upper, Set) → Set` |
| `HE` | minimum | `(Property, Set) → Set` |
| `SE` | maximum | `(Property, Set) → Set` |

`IL` retains entities whose property value is strictly between `lower` and `upper` (both bounds exclusive). `HE` retains all entities tied for the minimum value; `SE` retains all entities tied for the maximum.

## Direction and the ANG Property

A direction is expressed as two `Location` values — an origin and a point the ray passes through. No new Direction type is introduced. This is sufficient to define both a ray and an angular reference.

### ANG property root

`ANG(Location from, Location through) → Property` — the angular difference between an entity's position and the ray defined by `from` through `through`. Relational like `HOR`: an angle only exists relative to something.

### DAR — pointing direction as two locations

`DAR` is a `Location → [Location, Location]` rune. It consumes one `Location` from the token stream (the origin) and produces two `Location` values: the consumed origin and the point on the unit circle in the executor's pointing direction from that origin. This fills both `Location` arguments of `ANG` or `RAY` in a single token.

This requires a parser change: an expression may satisfy a span of N consecutive expected argument types if it produces exactly those N types in order. This is the general rule — `DAR` is the first instance of multi-value production, not a special case.

### DAN decomposed

`DAN` is a shorthand for:

```
HE RAY DAR
```

The closest entity along the ray defined by the caster's pointing direction from the caster's center. `RAY(Location from, Location through) → Property` is the along-ray distance property, engine-native like `HOR`.

## Replacements

| Old rune | Equivalent expression |
|----------|-----------------------|
| `HORO N` | `O HOR N` (origin defaults to `OH`) |
| `ZYHE S` | `HE ZY S` |
| `ZYSE S` | `SE ZY S` |
| `ZYIL S lower upper` | `IL ZY lower upper S` |
| `FUHE S` | `HE FU S` |
| `FUSE S` | `SE FU S` |
| `FUIL S lower upper` | `IL FU lower upper S` |
| `HORHE S origin` | `HE HOR origin S` |
| `HORSE S origin` | `SE HOR origin S` |
| `HORIL S lower upper origin` | `IL HOR origin lower upper S` |
