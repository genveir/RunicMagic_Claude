# Rune Set

This is the authoritative rune reference. The milestone spell and future canonical patterns
are listed at the bottom.

## Execution

| Rune | Meaning | Signature |
|------|---------|-----------|
| `ZU` | execute | (Statement) → ExecutableStatement |

ZU is a root rune. It consumes a Statement and applies it to the world.
Nothing can consume ExecutableStatement — ZU must appear first in the token stream.

## Effects

| Rune | Meaning | Signature |
|------|---------|-----------|
| `VUN` | push | (Set, Number, Location = PAR(OH)) → Statement |
| `VAR` | pull | (Set, Number, Location = PAR(OH)) → Statement |

## Power Sourcing

| Rune | Meaning | Signature |
|------|---------|-----------|
| `SHU` | with power source | (Set, Statement) → Statement |

SHU pushes an EntitySet onto the front of the power draw stack before executing a Statement, then pops it after. The default draw order (scope of executor → executor → scope of caster → caster) is extended at the front, not replaced — so a single `SHU X` produces the draw order X → scope of executor → executor → scope of caster → caster.

SHU can be nested. Each SHU pushes onto whatever stack is current at the point of its execution, so the innermost push draws first. Example: `SHU A (SHU B <statement>)` produces draw order B → A → scope of executor → executor → scope of caster → caster for `<statement>`.

## Sets

| Rune | Meaning | Signature |
|------|---------|-----------|
| `LA` | scope of | (Set = OH) → Set |
| `HORO` | near | (Number, Location = PAR(OH)) → Set |
| `ZYIL` | weight range filter | (Set, Number lower, Number upper) → Set |
| `ZYHE` | lightest | (Set) → Set |
| `ZYSE` | heaviest | (Set) → Set |
| `FUIL` | power range filter | (Set, Number lower, Number upper) → Set |
| `FUHE` | least powerful | (Set) → Set |
| `FUSE` | most powerful | (Set) → Set |
| `HORIL` | distance range filter | (Set, Number lower, Number upper, Location = PAR(OH)) → Set |
| `HORHE` | closest | (Set, Location = PAR(OH)) → Set |
| `HORSE` | farthest | (Set, Location = PAR(OH)) → Set |

`LA` maps each member of the input Set to its scope (via the entity's scope delegate) and returns the union. Defaults to `OH`, so bare `LA` maps the executor's scope.

`HORO` returns all entities in the world whose nearest bounding edge is within `Number` millimetres of `Location`. Defaults to the executor's position, so `HORO HET` selects everything within 1 mm of the executor.

`ZYIL` filters a Set to entities whose weight in grams is strictly greater than `lower` and strictly less than `upper` (both bounds exclusive).

`ZYHE` returns all entities in the Set tied for minimum weight. If multiple entities share the lowest weight, all are returned.

`ZYSE` returns all entities in the Set tied for maximum weight. If multiple entities share the highest weight, all are returned.

`FUIL` filters a Set to entities whose current power is strictly greater than `lower` and strictly less than `upper` (both bounds exclusive). Entities with no power reservoir are treated as having 0 current power.

`FUHE` returns all entities in the Set tied for minimum current power. Entities with no power reservoir are treated as having 0 current power.

`FUSE` returns all entities in the Set tied for maximum current power. Entities with no power reservoir are treated as having 0 current power.

`HORIL` filters a Set to entities whose distance from `Location` is strictly greater than `lower` and strictly less than `upper` (both bounds exclusive). Distance is measured from the nearest bounding edge; entities containing the origin have distance 0. Defaults to the executor's position.

`HORHE` returns all entities in the Set tied for minimum distance from `Location`. Defaults to the executor's position.

`HORSE` returns all entities in the Set tied for maximum distance from `Location`. Defaults to the executor's position.

## Invocation

| Rune | Meaning | Signature |
|------|---------|-----------|
| `GWYAH` | invoke | (Set) → Statement |

Activates all inscriptions found on entities in the Set. Returns a Statement that, when executed, fires those inscriptions in order. The inscribed-on entity becomes the executor of each activated inscription; the caster of the invoking spell becomes its caster.

## Entity References

| Rune | Meaning | Signature |
|------|---------|-----------|
| `A` | me | () → Set |
| `OH` | this | () → Set |
| `KAL` | touching | () → Set |
| `DAN` | pointing at | () → Set |

`A` produces a singleton Set containing the caster; `OH` produces a singleton Set containing the executor.

They are identical for spoken spells and diverge for inscribed spells (where the executor is the inscribed object).

Both are used as targeting references and, in the power sourcing context (RMC-15), as reservoir references.

`KAL` and `DAN` are conscious-action references set by the player via UI before casting. 
`KAL` is the entity the caster is deliberately indicating.
`DAN` is the entity the caster is pointing at.

## Location

| Rune | Meaning | Signature |
|------|---------|-----------|
| `PAR` | location of | (Set) → Location |
| `GER` | weighted centroid of | (Set) → Location |

`PAR` resolves a Set to the unweighted centroid of its members' bounding rectangles.

`GER` resolves a Set to the centroid weighted by each member's `Weight`. If all weights are zero, falls back to unweighted centroid.

## Numbers

### Literals

| Rune | Value |
|------|-------|
| `JON` | 0 |
| `HET` | 1 |
| `DET` | 2 |
| `TET` | 3 |
| `FET` | 5 |
| `SET` | 7 |
| `HOT` | 14¹ = 14 |
| `DOT` | 14² = 196 |
| `TOT` | 14³ = 2 744 |
| `FOT` | 14⁵ = 537 824 |
| `SOT` | 14⁷ = 105 413 504 |

All literals have signature `() → Number`. The base literals (`JON` through `SET`) are the primes up to 7, plus zero. The power literals (`HOT` through `SOT`) are 14 raised to those same prime exponents.

### Arithmetic

| Rune | Meaning | Signature |
|------|---------|-----------|
| `IR` | multiply | (Number A, Number B) → Number |
| `MO` | add | (Number A, Number B) → Number |
| `UIT` | modulo | (Number A, Number B) → Number |
| `EID` | integer divide | (Number A, Number B) → Number |
| `DEID` | halve | (Number A) → Number |
| `MOST` | one and a half | (Number A) → Number |

All arithmetic truncates toward zero. There are no fractional numbers in the system — every intermediate and final result is an integer.

---

## Canonical Spell Patterns

### Milestone spell — push everything touching the caster

```
ZU VUN LA TOT
```

Pushes all entities in the caster's local scope 2 744 mm (~2.7 m) away from the caster.
This is the target spell for the RMC-18 walking skeleton.

**Parse tree:**

```
ZU
└── VUN
    ├── LA
    │   └── [default] OH
    ├── TOT
    └── [default] PAR
        └── [default] OH
```

**Node by node:**

| Expression | Type | Value |
|------------|------|-------|
| `TOT` | Number | 2 744 |
| `OH` | Set | singleton set containing the executor |
| `LA OH` | Set | scope of the executor (default argument `OH` consumed implicitly) |
| `PAR OH` | Location | centroid of the executor's bounding rectangle (default argument `OH` consumed implicitly) |
| `VUN LA TOT PAR OH` | Statement | move each entity in the Set 2 744 mm away from the executor's centre |
| `ZU VUN …` | ExecutableStatement | execute the Statement |

Both `LA` and `PAR` use their declared defaults — neither `OH` appears in the token stream.

`LA` could be replaced with `LA OH` (explicit) or `LA A` (caster's scope) and the result
would be identical for this spell, since the caster is also the executor.

### Mini-milestone spell — push whatever you're pointing at

```
ZU VUN DAN TOT
```

Pushes the aimed-at entity 2 744 mm away from the caster. Identical in structure to the Milestone 1 spell with `DAN` substituted for `LA` — targeting a single conscious-action reference rather than the full executor scope.

### Milestone 2 spell — trigger the inscription on whatever you're pointing at

```
ZU GWYAH DAN
```

Activates all inscriptions on the entity the caster is pointing at.
This is the target spoken spell for the Milestone 2 scenario.

**Parse tree:**

```
ZU
└── GWYAH
    └── DAN
```

**Node by node:**

| Expression | Type | Value |
|------------|------|-------|
| `DAN` | Set | singleton set containing the aimed-at entity |
| `GWYAH DAN` | Statement | activate inscriptions on that entity |
| `ZU GWYAH DAN` | ExecutableStatement | execute the Statement |

It will execute an inscription on a rock inside a room, which will open the door to the room. The spell on this rock needs to be designed as part of the milestone.

### Future: cast the spell in context

```
ZU  BEH
```

Executes the inscriptions on the entity the caster is touching.
Requires BEH (future rune). Equivalent to `ZU GWYAH KAL` but central to the lore and cannot be changed.

### Future: protect power

```
BASDU  TI  OH
```

Disallows taking power from the executor. Requires BASDU and TI (future runes).
