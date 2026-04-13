# Runes

## Core Runes

| Rune | Meaning | Returns | Arg 1 | Arg 2 | Arg 3 |
|------|---------|---------|-------|-------|-------|
| TI | take power | PowerSource, Statement | from: PowerReservoir = A | amount: Number = IMO | |
| PAR | use power | Statement | for: Statement | using: PowerSource = TI | |
| OH | me | PowerReservoir (+) | | | |
| DZEJL | creature | PowerReservoir (+) | | | |
| VAZ | create | Statement | what: object | where: Position | |
| VUN | move | Statement | what: object | where: Position | |
| VYR | destroy | Statement | what: object | | |
| VAYOR | alter property | Statement | what: Property | new value: (propType) | |
| HOR | property of | Property | property: property | of: object = A | |

## Numbers

| Rune | Meaning | Returns | Arg 1 | Arg 2 |
|------|---------|---------|-------|-------|
| HET | one | Number | | |
| SJU | fourteen | Number | | |
| PETI | thousand | Number | | |
| MUR | add | Number | first: Number | second: Number |
| ETT | multiply | Number | first: Number | second: Number |
| IMO | requested | Number | | |

## Access Modifiers

| Rune | Meaning | Returns | Arg 1 |
|------|---------|---------|-------|
| MESDU | allow | ExecutedStatement | statement: Statement |
| BASDU | disallow | ExecutedStatement | statement: Statement |

## Control Flow

| Rune | Meaning | Returns | Arg 1 | Arg 2 |
|------|---------|---------|-------|-------|
| FYAR | foreach | Statement | who: Group | what: ExecutingStatement |
| TWYAR | assign | Statement | what: Expression | to: Variable |
| ZU | execute | ExecutedStatement | function: Statement | |
| SHRIK | on | ConditionalStatement | trigger: Action | what: ExecutingStatement |
| TUZUR | repeat execution | RepeatedStatement | function: ConditionalStatement | |
| RUN | the speaking of a rune | Action | which rune: runeword | where: LA |
| SJAR | damaging | Action | what: object = A | |

## Determiners

| Rune | Meaning | Returns | Arg 1 | Arg 2 |
|------|---------|---------|-------|-------|
| MJORNER | caster | Scope | | |
| A | this | Scope | | |
| LA | the local scope | Scope | | |
| VERLO | the global scope | Scope | | |
| NJEL | any | Group | out of: Group = BUZD | |
| BUZD | all | Group | of type: objectType | in scope: Scope = LA |
| DELON | with property | Group | property: property | out of: Group |
| BEH | the spell in context | Reference | | |
| DUMER | contained by | Area | container: object = A | |
| DWOR | in range (meters) | Area | anchor: object = A | range: Number = HET |

## Special Properties

| Rune | Meaning | Returns | Arg 1 |
|------|---------|---------|-------|
| ZUVAR | in context | boolean | of: object |
| ZUVER | in context | boolean | of: Scope |
| ZUVYR | in context | boolean | of: Spell |

## Person Properties

| Rune | Meaning | Returns |
|------|---------|---------|
| DROR | level of sustenance | percentage |
| VJUR | level of hydration | percentage |
| ORON | level of aging | percentage |
| ZORON | level of health | percentage |

## Positions

| Rune | Meaning | Returns | Arg 1 |
|------|---------|---------|-------|
| TJOL | up | Position | how far in cm: Number |
