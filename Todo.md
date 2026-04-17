# RunicMagic_Claude Todo

## To Do — Milestone 2

Next ticket number: RMC-49
Next bugfix number: BUG-3

| Key | Title | Description | Blocked By |
|-----|-------|-------------|------------|
| RMC-41 | DAN rune and pointing mechanic | Implement the DAN(pointing at) rune, which returns a singleton Set containing the entity the caster is consciously aiming at. Requires: a translucency capability on entities (windows are translucent, walls and doors are not); a line-of-sight algorithm that casts a ray from the caster and returns the first non-translucent entity hit; and a UI affordance (canvas click) to set the active aim target. | |
| RMC-46 | Move caster position in-game | Add a UI affordance to reposition the caster without touching the database. Useful for testing line-of-sight scenarios without reseeding. | |
| RMC-47 | 🚩 Mini-milestone — point at an entity and push it | With the milestone world seeded, the player clicks an entity to aim at it and casts `ZU VUN DAN FOTIR FOTIR FOTIR HET`. The aimed-at entity is pushed 2 744 mm away from the caster. | RMC-41 RMC-46 |
| RMC-15 | Design power sourcing implementation | The sourcing rules (explicit > executor-scope > caster-scope > local-scope, with preference ordering) are complex enough to deserve their own design and implementation ticket. | |
| RMC-16 | Design inscribed spells on objects | Spells inscribed on objects have different executor/caster semantics. Define how the world model represents inscribed spells, how they are activated, and how the evaluator handles the executor being an object rather than a creature. | |
| RMC-40 | GWYAH rune — invoke inscriptions on a Set | Implement the GWYAH(invoke) rune: takes a Set, activates all inscriptions found on entities in that Set, and returns a Statement. The entity the inscription is on becomes the executor; the caster of the invoking spell becomes the caster of the activated inscription. | RMC-16 |
| RMC-43 | Seed the milestone world | Build the seeded world for the Milestone 2 scenario: a room entity containing walls, windows (translucent), a door, and a rock with an inscription. The caster's starting position is randomised so they may or may not have line of sight to the rock through a window. | RMC-16 |
| RMC-44 | Design the inscription spell | Determine what the rock's inscription says. The spell must open the door when activated via GWYAH. Work out which runes are needed (filtering a Set to the door, the open/close effect, etc.) and raise any missing-rune tickets that fall out of the design. | RMC-16 RMC-40 |
| RMC-45 | 🏁 Milestone 2 — trigger an inscription through a window | A room contains walls, windows, a door, and a rock with an inscription. The caster is seeded at a random position — either with or without line of sight to the rock through a window. The player clicks the rock to aim, casts `ZU GWYAH DAN`, and the inscription on the rock opens the door. | RMC-15 RMC-16 RMC-37 RMC-38 RMC-39 RMC-40 RMC-41 RMC-43 RMC-44 |

## To Do — Other

| Key | Title | Description | Blocked By |
|-----|-------|-------------|------------|
| RMC-37 | Kill creatures when they run out of life | At any point during spell execution, living entities may run out of hitpoints. In this case the game should register that they're dead and remove their living and agency properties | |
| RMC-38 | Register entity destruction | It should be possible to destroy entities. When this happens, the game should register that the entity is destroyed | |
| RMC-39 | Stop spell execution on caster or executor death or destruction | When the caster or executor of a spell cease to be in an active state the spell should stop executing | RMC-37 RMC-38 |
| RMC-42 | KAL rune and touching mechanic | Implement the KAL(touching) rune, which returns a singleton Set containing the entity the caster is consciously touching. Requires a UI affordance to set the active touch target. Distinct from the automatic scope-of touching used by LA — this is a deliberate player action. | |
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
