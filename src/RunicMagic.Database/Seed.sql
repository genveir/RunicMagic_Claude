if db_name() != 'RunicMagic'
begin
    print 'ERROR: This script must be run in the RunicMagic database.';
    set noexec on;
end

delete from entitylife;
delete from entitycharge;
delete from entitylocomotion;
delete from inscription;
delete from entities;

declare @caster         uniqueidentifier = newid();
declare @guard          uniqueidentifier = newid();
declare @manaStone      uniqueidentifier = newid();
declare @smallManaStone uniqueidentifier = newid();
declare @rock           uniqueidentifier = newid();

insert into Entities (Id, EntityTypeId, Label, X, Y, Width, Height, HasAgency, Weight, Strength, IsTranslucent, Angle, MaxStructuralIntegrity, CurrentStructuralIntegrity, DragCoefficient, AngularDragCoefficient, GroundFrictionCoefficient)
values
    -- caster (outside the room; X,Y are center)
    (@caster,    1, 'Player',               12045,   4142, 300,  900,  1, 70000,    100000, 0, 0,                  1000, 1000, 0.4, 0.4, 0.4),
    -- mana stone (near top-right interior corner)
    (@manaStone, 2, 'Mana Stone',           8125,    925, 250,  250,  0, 3000,     0,      0, 0,                  1000, 1000, 0.2, 0.2, 0.2),
    -- bottom wall (low Y = bottom of screen), split by door
    (newid(),    3, 'Bottom Wall (Left)',    1600,    300, 3200, 600,  0, 19200000, 0,      0, 0,                  1000, 1000, 1.0, 1.0, 0.7),
    (newid(),    3, 'Door',                 3800,    300, 1200, 100,  0, 30000,    0,      0, 0,                  1000, 1000, 1.0, 1.0, 0.7),
    (newid(),    3, 'Bottom Wall (Right)',   6800,    300, 4800, 600,  0, 28800000, 0,      0, 0,                  1000, 1000, 1.0, 1.0, 0.7),
    -- left wall, split by one window near the top
    (newid(),    3, 'Left Wall (Lower)',      300,   4400, 600,  7600, 0, 45600000, 0,      0, 0,                  1000, 1000, 1.0, 1.0, 0.7),
    (newid(),    3, 'Left Window',            300,   8600, 100,  800,  0, 5000,     0,      1, 0,                  1000, 1000, 1.0, 1.0, 0.7),
    (newid(),    3, 'Left Wall (Upper)',      300,   9800, 600,  1600, 0, 9600000,  0,      0, 0,                  1000, 1000, 1.0, 1.0, 0.7),
    -- right wall, split by two windows
    (newid(),    3, 'Right Wall (Lower)',    8900,   2400, 600,  3600, 0, 21600000, 0,      0, 0,                  1000, 1000, 1.0, 1.0, 0.7),
    (newid(),    3, 'Right Window (Lower)', 8900,   4600, 100,  800,  0, 5000,     0,      1, 0,                  1000, 1000, 1.0, 1.0, 0.7),
    (newid(),    3, 'Right Wall (Middle)',   8900,   7000, 600,  4000, 0, 24000000, 0,      0, 0,                  1000, 1000, 1.0, 1.0, 0.7),
    (newid(),    3, 'Right Window (Upper)', 8900,   9400, 100,  800,  0, 5000,     0,      1, 0,                  1000, 1000, 1.0, 1.0, 0.7),
    (newid(),    3, 'Right Wall (Upper)',    8900,  10200, 600,  800,  0, 4800000,  0,      0, 0,                  1000, 1000, 1.0, 1.0, 0.7),
    -- top wall (high Y = top of screen)
    (newid(),    3, 'Top Wall',             4600,  10900, 9200, 600,  0, 55200000, 0,      0, 0,                  1000, 1000, 1.0, 1.0, 0.7),
    -- corridor walls
    (newid(),    3, 'Corridor Bottom Wall',    -2000, 2400, 600, 4800, 0, 28800000, 0,      0, 0,                  1000, 1000, 1.0, 1.0, 0.7),
    (newid(),    3, 'Corridor Top Wall',       -2000, 9000, 600, 4700, 0, 28200000, 0,      0, 0,                  1000, 1000, 1.0, 1.0, 0.7),
    -- guard (in the corridor)
    (@guard,     1, 'Guard',                -1000,  10000, 300,  900,  1, 70000,    100000, 0, 270 * PI() / 180,   1000, 1000, 0.4, 0.4, 0.4),
    -- small mana stone (next to the rock)
    (@smallManaStone, 2, 'Small Mana Stone', 5000, 5600, 100, 100, 0, 500,        0,      0, 0,                  1000, 1000, 0.2, 0.2, 0.2),
    -- rock (center of room, to be inscribed)
    (@rock,      3, 'Rock',                 4600,   5600, 700,  700,  0, 200000,   0,      0, 35 * PI() / 180,    1000, 1000, 0.8, 0.8, 0.6),
    (newid(),    3, 'Rotated Test Wall',    12000, 6000, 600,  5000, 0, 3000000,  0,      0, 70 * PI() / 180,    1000, 1000, 1.0, 1.0, 0.7);

insert into EntityLife (EntityId, MaxHitPoints, CurrentHitPoints)
values
    (@caster, 1000000000, 1000000000),
    (@guard, 1000000000, 1000000000);

insert into EntityCharge (EntityId, MaxCharge, CurrentCharge)
values
    (@manaStone, 10000000000, 10000000000),
    (@smallManaStone, 100000000, 100000000);

insert into EntityLocomotion (EntityId, LocomotionEfficiency)
values
    (@caster, 0.8),
    (@guard, 0.8);

insert into Inscription (EntityId, SpellText)
values
    (@rock, 'CJIR ZYHE LA ZYSE LA ZYSE HORO MOST TOT DEID DEID TOT'),
    (@rock, 'VAR ZYHE HORO IR TET DOT ZYSE LA ZYSE HORO MOST TOT MO EID TOT FET DET PAR ZYSE LA ZYSE HORO MOST TOT');

set noexec off;
