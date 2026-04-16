create table EntityTypes (
    Id   int          not null constraint PK_EntityTypes primary key,
    Name nvarchar(50) not null
);

insert into EntityTypes (Id, Name) values
    (1, 'Creature'),
    (2, 'ManaSource'),
    (3, 'Object');

create table Entities (
    Id           uniqueidentifier not null constraint PK_Entities primary key,
    EntityTypeId int              not null constraint FK_Entities_EntityTypes references EntityTypes (Id),
    Label        nvarchar(100)    not null,
    X            int              not null,
    Y            int              not null,
    Width        int              not null,
    Height       int              not null,
    HasAgency    bit              not null constraint DF_Entities_HasAgency default 0,
    Weight       int              not null
);

create table EntityLife (
    EntityId         uniqueidentifier not null constraint PK_EntityLife primary key
                                               constraint FK_EntityLife_Entities references Entities (Id),
    MaxHitPoints     int              not null,
    CurrentHitPoints int              not null
);

create table EntityCharge (
    EntityId      uniqueidentifier not null constraint PK_EntityCharge primary key
                                            constraint FK_EntityCharge_Entities references Entities (Id),
    MaxCharge     int              not null,
    CurrentCharge int              not null
);

-- Seed data for the RMC-18 walking skeleton scenario
-- Scene: caster standing at a doorway between two walls
-- Milestone spell: ZU VUN LA FOTIR FOTIR FOTIR HET
--   pushes all entities touching the caster 2744mm away

-- X and Y are center coordinates.
insert into Entities (Id, EntityTypeId, Label, X, Y, Width, Height, HasAgency, Weight) values
    ('a0000000-0000-0000-0000-000000000001', 3, 'Wall Left',  2500, 250, 5000, 500, 0, 5000000),
    ('a0000000-0000-0000-0000-000000000002', 3, 'Wall Right', 8400, 250, 5000, 500, 0, 5000000),
    ('a0000000-0000-0000-0000-000000000003', 3, 'Door',       5450, 225, 900,  100,  0, 30000),
    ('a0000000-0000-0000-0000-000000000004', 1, 'Caster',     5450, 425, 500,  300, 1, 70000);
--  ('a0000000-0000-0000-0000-000000000004', 1, 'Caster',     5650, 425, 500,  300, 1, 70000); -- touching Wall Right (scope includes door + wall right)

insert into EntityLife (EntityId, MaxHitPoints, CurrentHitPoints) values
    ('a0000000-0000-0000-0000-000000000004', 1000, 1000);
