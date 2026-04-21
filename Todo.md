# RunicMagic_Claude Todo

## To Do — Milestone 2

Next ticket number: RMC-73
Next bugfix number: BUG-4

| Key | Title | Description | Blocked By |
|-----|-------|-------------|------------|
| RMC-66 | CJIR and CJAR runes — rotate clockwise and counterclockwise | `(IEntitySet target, INumber angle, ILocation origin = PAR target) → IStatement`. CJIR rotates clockwise, CJAR counterclockwise, otherwise identical. Angle is in a 2744-degree circle (base-14 degrees; TOT = full circle). Default origin is the centroid of the rotating entity set itself — the parser substitutes the first argument's expression as the default for the third, so bare `CJIR A HET` rotates the caster one degree around its own centre. Cost is proportional to mass × arc distance; specifics to be determined during implementation. | |
| RMC-65 | TIORJ rune — fill | `(IEntitySet from, IEntitySet to, INumber amount) → IStatement`. Transfers up to `amount` power from entities in `from` to entities in `to`. Overcharge (recipient receiving more than its maximum) is a special concern that needs to be addressed in the design. | |
| RMC-44 | Design the inscription spell | Determine what the rock's inscription says. The spell must open the door when activated via GWYAH. Work out which runes are needed (filtering a Set to the door, the open/close effect, etc.) and raise any missing-rune tickets that fall out of the design. | RMC-40 |
| RMC-45 | 🏁 Milestone 2 — trigger an inscription through a window | A room contains walls, windows, a door, and a rock with an inscription. The player clicks the rock to aim, casts `ZU GWYAH DAN`, and the inscription on the rock opens the door if the caster has line of sight to the rock. | RMC-15 RMC-37 RMC-38 RMC-39 RMC-40 RMC-51 RMC-52 RMC-43 RMC-44 |

## To Do — Other

| Key | Title | Description | Blocked By |
|-----|-------|-------------|------------|
| RMC-60 | Design small items | Define the world model for small items — portable objects a creature can carry (e.g. a mana gem in the caster's pocket). Covers how items are represented, how carrying/inventory works, and how items interact with spells and power sourcing. | |
| RMC-37 | Kill creatures when they run out of life | At any point during spell execution, living entities may run out of hitpoints. In this case the game should register that they're dead and remove their living and agency properties | |
| RMC-38 | Register entity destruction | It should be possible to destroy entities. When this happens, the game should register that the entity is destroyed | |
| RMC-39 | Stop spell execution on caster or executor death or destruction | When the caster or executor of a spell cease to be in an active state the spell should stop executing | RMC-37 RMC-38 |
| RMC-14 | Design channeling and persistent effects | Written runes stay active while power is channeled. Define what "channeling" means mechanically — what keeps a spell alive, how it is terminated, and how the executor tracks ongoing effects. | |
| RMC-7 | Identify missing runes | There are no runes that manipulate entity sets, or effects other than pushing. Audit the full rune set for gaps needed to write meaningful spells. | |
| RMC-48 | Formalize movement with collision | VUN currently teleports entities to their destination. Replace this with movement through space: the entity travels along the push vector and stops when it hits an entity (a wall, door, etc.) rather than passing through it. | |

## In Progress
| Key | Title | Description | Remarks |
|-----|-------|-------------|---------|

## Ready For Review

| Key | Title |
|-----|-------|

## Done

| Key | Title |
|-----|-------|
| RMC-54 | Clarify entity property model — capabilities, state, and transient data |
| RMC-49 | Add pointing direction to entity model |
| RMC-55 | Add GER rune for weighted centering |
| RMC-56 | PlayerService knows which entity is the caster |
| RMC-50 | UI knows which entity is the caster |
| RMC-46 | Move caster position in-game |
| RMC-51 | UI point-to-aim interaction |
| RMC-53 | Add translucency capability to entity model and database |
| RMC-52 | DAN(pointing at) rune |
| RMC-47 | 🚩 Mini-milestone — point at an entity and push it |
| RMC-42 | KAL rune and touching mechanic | 
| BUG-3 | Terminal scrolling does not take resizable height into account |
| RMC-57 | SHU rune — explicit power source |
| RMC-43 | Seed the milestone world |
| RMC-15 | Design power sourcing implementation |
| RMC-16 | Design inscribed spells on objects |
| RMC-58 | Add inscription to the rock |
| RMC-59 | Expansion selectors cost power scaled by selected entities' max power |
| RMC-40 | GWYAH rune — invoke inscriptions on a Set |
| RMC-61 | HORO rune — spatial selector for nearby entities |
| RMC-62 | Work on the number system |
| RMC-70 | Tax peak selection breadth during set resolution |
| RMC-63 | ZYIL rune — weight range filter and weight singletons |
| RMC-64 | FUIL rune — power range filter and power singletons |
| RMC-69 | Proximity selectors — closest, farthest, distance range |
| RMC-68 | Is-alive filter rune |
| RMC-71 | PA rune — intersection of scopes |
| RMC-67 | Set operation runes — union, intersection, difference |
| RMC-72 | CRIYR(read inscription) rune |