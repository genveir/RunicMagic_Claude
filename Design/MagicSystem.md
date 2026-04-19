# Magic System

## Spell Forms

A spell can be **spoken** or **inscribed**.

- A **spoken** spell activates the moment the caster intends to stop speaking runes.
- An **inscribed** spell activates once when triggered. If the effect can be channeled, it remains active as long as power is supplied to it.

---

## Casting a Spell

When a spell activates, the following steps happen in order.

### 1. Establish Context

Two entity references are resolved at the start of every spell:

| Rune | Value |
|------|-------|
| `A(me)` | The caster: The entity that spoke or wrote the spell |
| `OH(this)` | The executor: The entity in whose context the spell runs |

The caster and executor are usually the same entity. They differ when a spell is inscribed: the caster is whoever activates the inscription, but the executor is the entity the spell is inscribed on.

### 2. Parse the Spell

The rune string is parsed into an expression tree. Each rune consumes its arguments from the remaining token stream. Arguments not of the expected type stop evaluation at that point — the invalid rune and everything after it is ignored.

Example: `ZU(execute) HET(one)` — `ZU` expects a `Statement`, but `HET` returns a `Number`. Only `ZU` is evaluated; the rest is ignored.

*(Parser design: RMC-12)*

### 3. Calculate Evaluation Cost

The evaluation cost is the total rune count — including all defaults filled in automatically — divided by 5, rounded down. This cost is paid before execution begins.

Example: a spell that expands to 11 runes including defaults costs 2 power.

### 4. Source Evaluation Power

Power is drawn to cover the evaluation cost. This power is always drawn, even when the spell will not execute. If it can't be drawn, the executor disintegrates.

### 5. Execute

The expression tree is evaluated against the world state. Effects are applied as expressions resolve, and each effect draws power proportional to its magnitude at the point it resolves. This execution cost is separate from and independent of the evaluation cost — it taxes what the spell *does*, not how it is structured.

Each effect-producing rune is responsible for defining its own execution cost. A fireball with radius 1 and a fireball with radius 1000 are the same spell structurally, and cost the same to evaluate; but the execution cost scales with the effect, so magnitude has real consequence.

All Sets are resolved at the time of their execution. If a previous part of a spell has altered state, that altered state is carried forward. Runes being executed always only see the state as it is when they're executed.

*(Evaluator design: RMC-28)*

---

## Sets and Targeting

The magic system operates on Sets — predicates over world entities evaluated at execution time. 

Sets are used both for targeting (what does this effect apply to?) and for power sourcing (where can power be drawn from?). An entity can only be affected by a spell if it appears in the Set the effect is applied to.

Sets can be derived several ways:

- **Entity references** — `A(me)` and `OH(this)` produce singleton Sets.
- **Scope expansion** — `LA(scope of)` maps a Set to the union of its members' scopes, where each entity's scope is defined by its `HasScope` capability (a cave's scope is its contents, a person's scope is what they are touching).
- **Spatial construction** — `HORO(near)` builds a Set from a spatial predicate: all entities within a given distance of a Location.
- **Set operations** — union, intersection, difference, and property filters are natural future rune primitives.

Sets are always evaluated at the point of execution against current world state. A Set referenced twice in a spell may yield different results if the world changed between the two evaluations.

---

## Power Sourcing

Power is drawn in three distinct ways during a spell:

1. **Evaluation cost** — paid upfront (step 4) before execution begins, based on rune count.
2. **Selection cost** — paid each time a Set is consumed by a rune that consumes a Set but doesn't produce one (i.e. not by filters or selectors). Cost is `ceil(MaxPower / 1000)` per entity in the resolved Set, excluding the caster and executor, who are always free to select. If the full cost cannot be met, the Set resolves to empty.
3. **Execution cost** — paid by each effect rune as it fires, scaled by the magnitude of the effect.

All three draws use the same cascade. The engine works through a cascade of power sources, calling `Draw(amount)` on each in turn and carrying forward any shortfall until the cost is met. A source returns however much it can actually provide — which may be less than asked. The engine never inspects a source's internals or queries its reserves; it just takes what it gets and moves on.

What it means to be drained is entirely up to the entity. A mana crystal depletes its stored charge; a creature loses life. The engine has no special cases for any of this — it is all self-defined by the entity via its `Reservoir` delegate.

*(Cascade ordering and preference rules: RMC-15)*

## Failure

If the spell is structurally invalid at a point in the expression tree, evaluation stops there. Any portion of the spell that was valid and evaluated before the failure still takes effect, and its cost is still paid.

If the evaluation cost cannot be met in full, the executor disintegrates.

The system makes no attempt to pre-validate whether a spell can be afforded before drawing begins. There are no guardrails — power is drawn, effects fire, and consequences follow. A spell that cannot be completed will still drain every source it can reach before failing.

*(Full failure mode design: RMC-13)*

---

## Channeling

Inscribed spells with ongoing effects remain active as long as power is continuously supplied. Spoken spells activate once and do not channel.

*(Channeling design: RMC-14)*
