create table EntityTypes (
    Id   int          not null constraint PK_EntityTypes primary key,
    Name nvarchar(50) not null
);

insert into EntityTypes (Id, Name) values
    (1, 'Creature'),
    (2, 'ManaSource'),
    (3, 'Object');

create table Entities (
    Id            uniqueidentifier not null constraint PK_Entities primary key,
    EntityTypeId  int              not null constraint FK_Entities_EntityTypes references EntityTypes (Id),
    Label         nvarchar(100)    not null,
    X             int              not null,
    Y             int              not null,
    Width         int              not null,
    Height        int              not null,
    HasAgency     bit              not null constraint DF_Entities_HasAgency default 0,
    Weight        int              not null,
    IsTranslucent bit              not null constraint DF_Entities_IsTranslucent default 0
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