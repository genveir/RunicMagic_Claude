delete from entitylife;
delete from entitycharge;
delete from inscription;
delete from entities;

declare @caster        uniqueidentifier = newid();
declare @manaStone     uniqueidentifier = newid();
declare @smallManaStone uniqueidentifier = newid();
declare @rock          uniqueidentifier = newid();

insert into Entities (Id, EntityTypeId, Label, X, Y, Width, Height, HasAgency, Weight, IsTranslucent, Angle)
values
    -- caster (outside the room, above the top wall; X,Y are center)
    (@caster,    1, 'Player',               12045,   4142, 900,  300,  1, 70000,    0, 0),
    -- mana stone (near top-right interior corner)
    (@manaStone, 2, 'Mana Stone',           8125,    925, 250,  250,  0, 3000,     0, 0),
    -- top wall, split by door
    (newid(),    3, 'Top Wall (Left)',       1600,    300, 3200, 600,  0, 19200000,  0, 0),
    (newid(),    3, 'Door',                 3800,    300, 1200, 100,  0, 30000,     0, 0),
    (newid(),    3, 'Top Wall (Right)',      6800,    300, 4800, 600,  0, 28800000,  0, 0),
    -- left wall, split by one window near the bottom
    (newid(),    3, 'Left Wall (Upper)',      300,   4400, 600,  7600, 0, 45600000,  0, 0),
    (newid(),    3, 'Left Window',            300,   8600, 100,  800,  0, 5000,      1, 0),
    (newid(),    3, 'Left Wall (Lower)',      300,   9800, 600,  1600, 0, 9600000,   0, 0),
    -- right wall, split by two windows
    (newid(),    3, 'Right Wall (Upper)',    8900,   2400, 600,  3600, 0, 21600000,  0, 0),
    (newid(),    3, 'Right Window (Upper)', 8900,   4600, 100,  800,  0, 5000,      1, 0),
    (newid(),    3, 'Right Wall (Middle)',   8900,   7000, 600,  4000, 0, 24000000,  0, 0),
    (newid(),    3, 'Right Window (Lower)', 8900,   9400, 100,  800,  0, 5000,      1, 0),
    (newid(),    3, 'Right Wall (Lower)',    8900,  10200, 600,  800,  0, 4800000,   0, 0),
    -- bottom wall
    (newid(),    3, 'Bottom Wall',          4600,  10900, 9200, 600,  0, 55200000,  0, 0),
    -- small mana stone (next to the rock)
    (@smallManaStone, 2, 'Small Mana Stone', 5000, 5600, 100, 100, 0, 500,    0, 0),
    -- rock (center of room, to be inscribed)
    (@rock,      3, 'Rock',                 4600,   5600, 700,  700,  0, 200000,   0, 35 * PI() / 180);

insert into EntityLife (EntityId, MaxHitPoints, CurrentHitPoints)
values
    (@caster, 1000, 1000);

insert into EntityCharge (EntityId, MaxCharge, CurrentCharge)
values
    (@manaStone, 10000, 10000),
    (@smallManaStone, 100, 100);

insert into Inscription (EntityId, SpellText)
values
    (@rock, 'VUN ZYHE LA ZYSE LA ZYSE HORO MOST TOT TOT');
