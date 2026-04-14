# Magic System

## Spell Forms

A spell can be **spoken** or **inscribed**.

- A **spoken** spell activates the moment the caster intends to stop speaking runes.
- An **inscribed** spell activates once when triggered (e.g. by touch, or by another spell referencing it via `BEH(the spell in context)`). If the effect can be channeled, it remains active as long as power is supplied to it.

---

## Casting a Spell

When a spell activates, the following steps happen in order.

### 1. Establish Variables

Three variables are set at the start of every spell:

| Variable | Rune | Value |
|----------|------|-------|
| Caster | `MJORNER(caster)` | The entity that spoke or wrote the spell |
| Executor | `A(this)` | The entity in whose context the spell runs |
| Local scope | `LA(local scope)` | Defaults to the executor unless overridden |

The caster and executor are usually the same entity. They differ when a spell is inscribed: the caster is whoever activates the inscription, but the executor is the entity the spell is inscribed on.

Local scope begins as the scope of the executor. It can be reassigned within the spell using `TWYAR(assign)`, typically to an area defined by `DWOR(in range)` or `DUMER(contained by)`.

### 2. Parse the Spell

The rune string is parsed into an expression tree. Each rune consumes its arguments from the remaining token stream. Arguments not of the expected type stop evaluation at that point — the invalid rune and everything after it is ignored.

Example: `ZU(execute) MUR(add) MUR(add) HET(one) HET(one) HET(one)` — `ZU(execute)` expects a `Statement`, but `MUR(add)` returns a `Number`. Only `ZU(execute)` is evaluated; the rest is ignored.

*(Parser design: RMC-12)*

### 3. Calculate Cost

The evaluation cost is the total rune count — including all defaults filled in automatically — divided by 5, rounded down. This cost is paid before execution begins.

Example: a spell that expands to 11 runes including defaults costs 2 power.

### 4. Source Power

Power is drawn to cover the evaluation cost.

*(Power sourcing design: RMC-15, RMC-23)*

### 5. Execute

The expression tree is evaluated against the world state. Effects are applied as expressions resolve.

*(Evaluator design: RMC-8)*

---

## Failure

If the spell is structurally invalid at a point in the expression tree, evaluation stops there. Any portion of the spell that was valid and evaluated before the failure still takes effect, and its cost is still paid.

If the evaluation cost cannot be met in full, the executor disintegrates.

*(Full failure mode design: RMC-13)*

---

## Channeling

Inscribed spells with ongoing effects remain active as long as power is continuously supplied. Spoken spells activate once and do not channel.

*(Channeling design: RMC-14)*
