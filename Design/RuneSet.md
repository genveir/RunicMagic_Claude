# Rune Set

This is the authoritative rune reference. The milestone spell and future canonical patterns
are listed at the bottom.

## Execution

| Rune | Meaning | Signature |
|------|---------|-----------|
| `ZU` | execute | (Statement) → ExecutableStatement |

ZU is the root of every spell. It consumes a Statement and applies it to the world.
Nothing can consume ExecutableStatement — ZU must appear first in the token stream.

## Effects

| Rune | Meaning | Signature |
|------|---------|-----------|
| `VUN` | push | (Group, Number, Location = PAR(A)) → Statement |

Moves every entity in the Group away from the given Location by the given Number of
centimetres. Direction is determined per entity as the unit vector from Location to
the entity's centre. The default origin is the caster's position.

Execution cost scales with distance × number of entities pushed.

## Selection

| Rune | Meaning | Signature |
|------|---------|-----------|
| `BUZD` | all | (Scope) → Group |

Selects every entity currently in the given Scope.

## Scope

| Rune | Meaning | Signature |
|------|---------|-----------|
| `LA` | scope of | (Entity = OH) → Scope |

Resolves an Entity to its scope by calling the entity's scope delegate. Defaults to `OH`,
so bare `LA` is equivalent to `LA OH`. `LA A` gives the caster's scope.

## Entity References

| Rune | Meaning | Signature |
|------|---------|-----------|
| `A` | me | () → Entity |
| `OH` | this | () → Entity |

`A` produces the caster entity; `OH` produces the executor entity. They are identical for
spoken spells and diverge for inscribed spells (where the executor is the inscribed object).
Both are used as targeting references and, in the power sourcing context (RMC-15), as
reservoir references.

## Location

| Rune | Meaning | Signature |
|------|---------|-----------|
| `PAR` | location of | (Entity) → Location |

Resolves an Entity to its centre point at evaluation time.

## Numbers

| Rune | Meaning | Signature |
|------|---------|-----------|
| `HET` | one | () → Number |
| `FOTIR` | times fourteen | (Number) → Number |

Numbers are expressed as powers of 14. Representative values:

| Expression | Value |
|------------|-------|
| `HET` | 1 |
| `FOTIR HET` | 14 |
| `FOTIR FOTIR HET` | 196 |
| `FOTIR FOTIR FOTIR HET` | 2 744 |
| `FOTIR FOTIR FOTIR FOTIR HET` | 38 416 |

---

## Canonical Spell Patterns

### Milestone spell — push everything touching the caster

```
ZU  VUN  BUZD  LA  FOTIR FOTIR HET
```

Pushes all entities in the caster's local scope 196 cm away from the caster.
This is the target spell for the RMC-18 walking skeleton.

**Parse tree:**

```
ZU
└── VUN
    ├── BUZD
    │   └── LA
    ├── FOTIR
    │   └── FOTIR
    │       └── HET
    └── [default] PAR
        └── A
```

**Node by node:**

| Expression | Type | Value |
|------------|------|-------|
| `HET` | Number | 1 |
| `FOTIR HET` | Number | 14 |
| `FOTIR FOTIR HET` | Number | 196 |
| `LA` | Scope | scope of the executor (default argument `A` consumed implicitly) |
| `BUZD LA` | Group | all entities in that scope |
| `A` | Entity | the caster |
| `PAR A` | Location | centre point of the caster's bounding rectangle |
| `VUN BUZD LA  FOTIR FOTIR HET  PAR A` | Statement | move each entity in the Group 196 cm away from the caster's centre |
| `ZU VUN …` | ExecutableStatement | execute the Statement |

The third argument to VUN (`PAR A`) is not present in the token stream — the parser
substitutes it as the declared default when no Location-producing token follows the Number.

`LA` could be replaced with `LA OH` (explicit) or `LA A` (caster's scope) and the result
would be identical for this spell, since the caster is also the executor. They diverge
only in inscribed spells.

### Future: cast the spell in context

```
ZU  BEH
```

Executes the spell currently in context (e.g. an inscribed spell being activated).
Requires BEH (future rune).

### Future: protect power

```
BASDU  TI  OH
```

Disallows taking power from the executor. Requires BASDU and TI (future runes).
