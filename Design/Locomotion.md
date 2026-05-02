# Locomotion System Design Document

## Philosophy

This locomotion system is **property-based rather than stat-based**. Instead of defining creature movement through abstract game stats (speed, agility, etc.), movement behaviour emerges from a small set of physical properties interacting with environmental parameters. The goal is that novel interactions — terrain, load, injury, weather — should produce correct and intuitive results without requiring explicit rules for each case.

A stats system has to anticipate every interaction and write a rule for it. A physics-based property system just has to be internally consistent. Interactions you never explicitly designed tend to come out right anyway.

---

## Core Creature Properties

### Strength
The raw force a creature can apply through its locomotion system. Combines muscle mass, fibre composition, and mechanical advantage of the skeleton. This is a *current* value, not a fixed one — it degrades with fatigue and injury, and all downstream effects follow automatically.

### Weight
The creature's total weight including carried load. Heavier creatures require more force to accelerate and are penalised more on slopes and soft ground. Load bearing is simply an addition to this value.

### Drag Coefficient
The creature's aerodynamic resistance profile. Less significant at low speeds but becomes the dominant resistive force at high speeds (scales with v²). A crouched or streamlined creature has a lower coefficient. Relevant primarily at speeds above ~10 m/s.

### Locomotion Efficiency
A coefficient (0–1) representing how well the creature converts raw strength into forward motion. This is the most behaviourally rich property in the system. It captures:

- **Ground contact quality** — continuous contact (wheels) is near 1.0; legged locomotion is inherently lower due to impact losses and the need to repeatedly arrest and re-launch limbs
- **Gait optimisation** — a galloping spine-flexing quadruped is more efficient than a bounding biped at speed
- **Limb mass distribution** — lighter distal limbs waste less energy on swing
- **Tendon elasticity** — energy recovered from elastic storage increases effective efficiency

Efficiency is also the primary surface interaction parameter. Ice, mud, and water reduce it directly. The creature's underlying properties still matter — a stronger, lighter creature is still faster on ice — but the ceiling is dramatically lower.

### CoM Height / Stance Width Ratio
The ratio of the creature's centre of mass height to its effective stance width. This governs toppling risk in turns. Low, wide creatures can sustain higher lateral acceleration without rolling over. This value falls naturally out of creature geometry if modelled physically, and does not need to be set independently.

### Traction Coefficient
The maximum lateral force the creature can apply before slipping. Governs turning at speed. Analogous to locomotion efficiency but for sideways force. Can be the same parameter or a closely related one depending on implementation.

### Gait Inertia (speed-dependent)
A scalar representing how committed the creature is to its current movement pattern. Increases with speed. A creature at full gallop has high gait inertia — it cannot redirect instantly without losing speed or risking a fall. This gives the correct emergent behaviour: agility degrades naturally with speed, without requiring a separate agility stat.

---

## Derived Behaviours

### Straight-Line Speed and Acceleration
```
max_acceleration = (strength / weight) × efficiency
drag_force = drag_coefficient × v²
top_speed = velocity at which drag_force = max_drive_force
```

Heavier creatures accelerate slower. Higher efficiency raises both acceleration and top speed. Drag sets a terminal velocity that scales with the square root of available drive force divided by drag coefficient.

### Turning
Turning is governed by centripetal force requirements:

```
centripetal_acceleration = v² / r
max_centripetal_acceleration = traction × (1 / CoM_ratio) × (1 / gait_inertia(v))
minimum_turn_radius = v² / max_centripetal_acceleration
```

Turn radius scales with v², which naturally produces the correct result: creatures are much more agile at low speed than high speed. No special-casing required.

### Speed vs. Manoeuvrability Tradeoff
A creature can operate in a high-efficiency gait (maximum speed, high gait inertia) or a lower-efficiency gait (reduced top speed, lower gait inertia, better turning). This mirrors the real biological behaviour where agile animals switch from gallop to trot or bound when needing to turn. This can be an explicit creature decision or an automatic gait-selection system.

---

## Environmental Interactions

All environmental effects are expressed as modifications to the core properties above. No special locomotion rules are needed per environment.

| Environment | Effect |
|---|---|
| Ice | Traction coefficient ↓↓, Locomotion efficiency ↓ |
| Mud / Sand / Snow | Efficiency ↓ (scales with weight × contact area — heavy creatures penalised more) |
| Water | Drag coefficient ↑↑, Traction → ~0, partial buoyancy offsets weight |
| Uphill slope | Effective gravity component opposes motion, heavier creatures penalised more |
| Downhill slope | Effective gravity component assists motion, heavier creatures gain more |

The key insight for surface deformation (mud, sand, snow) is that ground pressure scales with **weight per contact area**. A heavier creature sinks in; a lighter one skips across. This falls out of the physics without special casing, provided contact area is modelled per creature.

---

## Load and Injury

**Carried load** is simply added to effective weight. This immediately and correctly degrades acceleration, top speed, turn radius, and slope performance simultaneously.

**Fatigue and injury** reduce current strength. A wounded creature is not "debuffed" by a rule — it simply has less strength available, and all downstream consequences follow. A leg injury might additionally reduce efficiency and traction on that side, producing a limp as an emergent consequence rather than an animation flag.

---

## Summary of Properties

| Property | What it captures | Primary interactions |
|---|---|---|
| Strength | Raw force output | Acceleration, slope, load |
| Weight | Inertia and ground pressure | Acceleration, slope, soft ground, load |
| Drag Coefficient | Aerodynamic resistance | Top speed (especially at high v) |
| Locomotion Efficiency | Ground contact quality, gait, elasticity | Top speed, acceleration, surface type |
| Traction Coefficient | Lateral force limit | Turning, surface type |
| CoM Height / Stance Width | Toppling risk | Turning radius |
| Gait Inertia (f(v)) | Commitment to current movement | Turning radius, agility at speed |
