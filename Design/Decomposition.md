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
| `HOR` | distance from origin | `(Set origin) → Property` |

`HOR` takes an origin `Set` because distance is not an intrinsic property of an entity — it only exists as a relation between two things. `HOR OH` and `HOR DAN` are genuinely different properties.

## Filter Runes

Filters operate on an already-bounded input `Set`. They do not default to `GA` — the caller is responsible for scoping the candidate pool first.

| Rune | Meaning | Signature |
|------|---------|-----------|
| `O` | less than | `(Property, Number threshold, Set) → Set` |
| `IL` | range filter | `(Property, Number lower, Number upper, Set) → Set` |
| `HE` | minimum | `(Property, Set) → Set` |
| `SE` | maximum | `(Property, Set) → Set` |

`O` retains all entities whose property value is strictly less than `threshold`. `IL` retains entities whose property value is strictly between `lower` and `upper` (both bounds exclusive). `HE` retains all entities tied for the minimum value; `SE` retains all entities tied for the maximum.

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

## Engine Selector Primitives

Each engine selection method maps to a named rune. These are the only points where the magic system reaches the engine's spatial index — all other Set construction is filtering on top of a primitive result. When a new engine selection method is introduced, a corresponding named rune must accompany it. Property filter runes (`O`, `HE`, `IL`, `SE`) are never a substitute for a missing primitive — they can only filter a set that a primitive already produced.

| Engine method | Rune | Notes |
|---------------|------|-------|
| `GetAll` | `GA` | Implemented |
| `GetAllInRangeFrom` | `HORO` (to be renamed, RMC-132) | Surface-to-surface from origin Set |
| `GetAllInRay` | — | No rune yet |
| `GetFirstInRay` | — | No rune yet |
| `GetInAllInRayExceptSourceEntities` | — | No rune yet |
| `GetUnionScope` | `LA` | Implemented; defined in RuneSet.md |
| `GetIntersectScope` | `PA` | Implemented; defined in RuneSet.md |

Cone cast runes will be added in RMC-74 

## Replacements

| Old rune | Equivalent expression |
|----------|-----------------------|
| `HORO N` | `[HORO rename] N` (direct rename, name TBD); decomposed: `O HOR OH N GA` |
| `ZYHE S` | `HE ZY S` |
| `ZYSE S` | `SE ZY S` |
| `ZYIL S lower upper` | `IL ZY lower upper S` |
| `FUHE S` | `HE FU S` |
| `FUSE S` | `SE FU S` |
| `FUIL S lower upper` | `IL FU lower upper S` |
| `HORHE S origin` | `HE HOR origin S` |
| `HORSE S origin` | `SE HOR origin S` |
| `HORIL S lower upper origin` | `IL HOR origin lower upper S` |
