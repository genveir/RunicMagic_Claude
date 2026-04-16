# Magic System

## Spell Forms

A spell can be **spoken** or **inscribed**.

- A **spoken** spell activates the moment the caster intends to stop speaking runes.
- An **inscribed** spell activates once when triggered (e.g. by touch, or by another spell referencing it via `BEH(the spell in context)`). If the effect can be channeled, it remains active as long as power is supplied to it.

---

## Casting a Spell

When a spell activates, the following steps happen in order.

### 1. Establish Context

Two entity references are resolved at the start of every spell:

| Rune | Value |
|------|-------|
| `A(me)` | The entity that spoke or wrote the spell |
| `OH(this)` | The entity in whose context the spell runs |

The caster and executor are usually the same entity. They differ when a spell is inscribed: the caster is whoever activates the inscription, but the executor is the entity the spell is inscribed on.

`LA(scope of)` takes an Entity and returns its scope. It defaults to `OH`, so bare `LA` is equivalent to `LA OH` — the executor's scope.

### 2. Parse the Spell

The rune string is parsed into an expression tree. Each rune consumes its arguments from the remaining token stream. Arguments not of the expected type stop evaluation at that point — the invalid rune and everything after it is ignored.

Example: `ZU(execute) MUR(add) MUR(add) HET(one) HET(one) HET(one)` — `ZU(execute)` expects a `Statement`, but `MUR(add)` returns a `Number`. Only `ZU(execute)` is evaluated; the rest is ignored.

*(Parser design: RMC-12)*

### 3. Calculate Evaluation Cost

The evaluation cost is the total rune count — including all defaults filled in automatically — divided by 5, rounded down. This cost is paid before execution begins.

Example: a spell that expands to 11 runes including defaults costs 2 power.

### 4. Source Evaluation Power

Power is drawn to cover the evaluation cost. 

### 5. Execute

The expression tree is evaluated against the world state. Effects are applied as expressions resolve, and each effect draws power proportional to its magnitude at the point it resolves. This execution cost is separate from and independent of the evaluation cost — it taxes what the spell *does*, not how it is structured.

Each effect-producing rune is responsible for defining its own execution cost. A fireball with radius 1 and a fireball with radius 1000 are the same spell structurally, and cost the same to evaluate; but the execution cost scales with the effect, so magnitude has real consequence.

*(Evaluator design: RMC-8)*

---

## Sets and Targeting

The magic system operates on Sets — predicates over world entities evaluated at execution
time. There is no distinction between "scope" and "group"; everything is a Set.

Sets are used both for targeting (what does this effect apply to?) and for power sourcing
(where can power be drawn from?). An entity can only be affected by a spell if it appears
in the Set the effect is applied to.

Sets can be derived several ways:

- **Entity references** — `A(me)` and `OH(this)` produce singleton Sets.
- **Scope expansion** — `LA` maps a Set to the union of its members' scopes, where each
  entity's scope is defined by its `HasScope` capability (a cave's scope is its contents,
  a person's scope is what they are touching).
- **Spatial construction** — future runes such as `DWOR(in range)` or `DUMER(contained by)`
  build Sets from spatial predicates.
- **Set operations** — union, intersection, difference, and property filters are natural
  future rune primitives.

Sets are always evaluated at the point of execution against current world state. A Set
referenced twice in a spell may yield different results if the world changed between the
two evaluations.

---

## Power Sourcing

Power is drawn twice per spell: once upfront to cover the evaluation cost (step 4), and once per effect as it resolves during execution (step 5). Both draws use the same cascade and the same `Draw` interface — the sourcing mechanism does not distinguish between the two.

The engine works through a cascade of power sources, calling `Draw(amount)` on each in turn and carrying forward any shortfall until the cost is met. A source returns however much it can actually provide — which may be less than asked. The engine never inspects a source's internals or queries its reserves; it just takes what it gets and moves on.

What it means to be drained is entirely up to the entity. A mana crystal depletes its stored charge; a creature loses life. The engine has no special cases for any of this — it is all self-defined by the entity via its `IsReservoir` capability.

*(Cascade ordering and preference rules: RMC-15)*

## Failure

If the spell is structurally invalid at a point in the expression tree, evaluation stops there. Any portion of the spell that was valid and evaluated before the failure still takes effect, and its cost is still paid.

If the evaluation cost cannot be met in full, the executor disintegrates.

*(Full failure mode design: RMC-13)*

---

## Channeling

Inscribed spells with ongoing effects remain active as long as power is continuously supplied. Spoken spells activate once and do not channel.

*(Channeling design: RMC-14)*
