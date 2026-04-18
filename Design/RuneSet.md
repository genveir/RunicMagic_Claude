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
| `VUN` | push | (Set, Number, Location = PAR(A)) → Statement |
| `VAR` | pull | (Set, Number, Location = PAR(A)) → Statement |

## Power Sourcing

| Rune | Meaning | Signature |
|------|---------|-----------|
| `SHU` | with power source | (Set, Statement) → Statement |

SHU pushes an EntitySet onto the front of the power draw stack before executing a Statement, then pops it after. The default draw order (scope of executor → executor → scope of caster → caster) is extended at the front, not replaced — so a single `SHU X` produces the draw order X → scope of executor → executor → scope of caster → caster.

SHU can be nested. Each SHU pushes onto whatever stack is current at the point of its execution, so the innermost push draws first. Example: `SHU A (SHU B <statement>)` produces draw order B → A → scope of executor → executor → scope of caster → caster for `<statement>`.

VUN moves every entity in the Set away from the given Location by the given Number of millimetres. VAR moves every entity towards it.

Direction is determined per entity as the unit vector from Location to the entity's centre. The default origin is the caster's position.

Execution cost scales with distance × summed weight of entities pushed.

## Sets

| Rune | Meaning | Signature |
|------|---------|-----------|
| `LA` | scope of | (Set = OH) → Set |

Maps each member of the input Set to its scope (via the entity's scope delegate) and returns the union. 

Defaults to `OH`, so bare `LA` maps the executor's scope.

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

Resolves a Set to the centroid of its members' bounding rectangles.

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
ZU VUN LA FOTIR FOTIR FOTIR HET
```

Pushes all entities in the caster's local scope 2 744 mm (~2.7 m) away from the caster.
This is the target spell for the RMC-18 walking skeleton.

**Parse tree:**

```
ZU
└── VUN
    ├── LA
    │   └── [default] OH
    ├── FOTIR
    │   └── FOTIR
    │       └── FOTIR
    │           └── HET
    └── [default] PAR
        └── A
```

**Node by node:**

| Expression | Type | Value |
|------------|------|-------|
| `HET` | Number | 1 |
| `FOTIR HET` | Number | 14 |
| `FOTIR FOTIR HET` | Number | 196 |
| `FOTIR FOTIR FOTIR HET` | Number | 2 744 |
| `OH` | Set | singleton set containing the executor |
| `LA OH` | Set | scope of the executor (default argument `OH` consumed implicitly) |
| `A` | Set | singleton set containing the caster |
| `PAR A` | Location | centroid of the caster's bounding rectangle |
| `VUN LA  FOTIR FOTIR FOTIR HET  PAR A` | Statement | move each entity in the Set 2 744 mm away from the caster's centre |
| `ZU VUN …` | ExecutableStatement | execute the Statement |

Both `LA` and `PAR A` use their declared defaults — neither `OH` nor `A` appears in the
token stream.

`LA` could be replaced with `LA OH` (explicit) or `LA A` (caster's scope) and the result
would be identical for this spell, since the caster is also the executor.

### Mini-milestone spell — push whatever you're pointing at

```
ZU VUN DAN FOTIR FOTIR FOTIR HET
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
