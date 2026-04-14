# Runic Magic

## Process to Cast a Spell

A spell is inert until an executing rune is written or spoken. Most commonly **ZU**. Spoken runes only activate once the speaker intends to stop speaking runes. Written runes only activate once, but if the effect can be channeled, they will remain active as long as power is channeled to it.

Once the spell is active:

1. Determine caster, put in "caster" variable (**MJORNER**)
2. Determine executor, put in "this" variable (**A**). This can be different from the caster — for example if a spell is inscribed on an object and *Numar* touches it and speaks **ZU BEH**, then *Numar* is caster of both **ZU BEH** and the referenced spell. *Numar* is also the executor of **ZU BEH**, but the executor of the inscribed spell is the object it is inscribed on.
3. Local scope is a room, if the executor is in a room. If the executor is in two rooms (for example, it is a door), the scope is both rooms. A room can be of any size — magic does not intrinsically care about physical size. If the executor is not in a room, a local scope must be explicitly defined or it is the same as the executor. Put in the "local scope" variable (**LA**)
4. Global scope is the entire plane of existence. Put in the "global scope" variable (**VERLO**). Breaking out of global scope is not possible except for gods, who have access to powers not available to mortals. In practice, this is a constant for any mortal and cannot be altered.
5. Evaluate the spell and pay the cost. If the spell is not valid or not executable, [the spell fails].

Power required to evaluate a spell is the integer division of the number of runes by 5, including defaults.

For example, to evaluate the spell:

```
ZU PAR BASDU TI TI NJEL BUZD DZEJL
```

which, with defaults, expands to:

```
ZU PAR BASDU TI A IMO TI NJEL BUZD DZEJL LA
```

The evaluation cost is 2, because the expanded size is 11, even though there are only 8 runes in the original spell. This cost will be extracted immediately.

If there are runes in the spell that are in an invalid position, they are not evaluated.

For example, to evaluate the spell:

```
ZU MUR MUR HI HI HI
```

The evaluation cost is 0, because **ZU** takes an argument of type Statement, and **MUR** is of type Number. The effective number of runes is 1, since only **ZU** will be evaluated. Any words that are not valid runes will also stop evaluation.

If the evaluation cost is not met, the executor disintegrates.

## Power Sourcing

When power is required, it will use, in order:

- Power sources specified in the spell, from outermost to innermost in order of definition
- Power sources in scope of the executor
- Power sources in scope of the caster
- Power sources in scope of the local scope
- Power sources in scope of the global scope (for most casters, this is disallowed)

If one of these is a power source it is in scope of itself. Any item touched by executor or caster is in scope of executor or caster. Any item contained in a local scope or global scope is in that scope.

By default:

- Large sources are preferred over small sources
- Depleting a source is preferred
- Non-life power is preferred over life power

Sources can be specified as not-allowed by stronger-scoped spells.

### Example

Take a spell that will require a lot of power, cast in a giant cave containing a great bonfire, several smaller fires, and a group of villagers. The caster, *Numar*, has a strong power stone ALPHA on which the spell is inscribed. ALPHA is lying in a fire. The spell specifically states that another power stone BETA nearby is the primary power source. BETA is lying in a fire. It also has a clause somewhere that draws power from a named power stone GAMMA which is outside the local scope (in *Numar*'s house back at the village). *Numar* has an activated tattoo which reads **BASDU TI OH** — "Do not allow taking power from me."

The spell will:

- Drain the BETA power stone, because it is the outermost power source specified in the spell
- Drain the GAMMA power stone, because it is mentioned in the spell, even if it was not for this purpose — it does not matter that it is not in scope, since it's directly referenced by the spell
- Drain the ALPHA power stone, because it is the strongest power source in scope of the executor
- Drain the fire ALPHA is in, because it is in scope of the executor
- Drain any fire, power stones, or other power sources that *Numar* is carrying, because they are in scope of the caster
- Drain the large bonfire, since it's the largest power source in the local scope
- Drain the smaller bonfires one by one, since they are the largest non-life power sources in the local scope
- Drain the villagers and any animals one by one, from strongest to weakest

*Numar* will not be killed because of his activated tattoo that disallows the spell to take his life. The fire BETA is lying in will only be snuffed once all fires are getting snuffed, and not immediately, because scope only extends by touch from caster and executor, and BETA is neither.
