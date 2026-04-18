create table EntityTypes (
    Id   bigint       not null constraint PK_EntityTypes primary key,
    Name nvarchar(50) not null
);

insert into EntityTypes (Id, Name) values
    (1, 'Creature'),
    (2, 'ManaSource'),
    (3, 'Object');

create table Entities (
    Id            uniqueidentifier not null constraint PK_Entities primary key,
    EntityTypeId  bigint           not null constraint FK_Entities_EntityTypes references EntityTypes (Id),
    Label         nvarchar(100)    not null,
    X             bigint           not null,
    Y             bigint           not null,
    Width         bigint           not null,
    Height        bigint           not null,
    HasAgency     bit              not null constraint DF_Entities_HasAgency default 0,
    Weight        bigint           not null,
    IsTranslucent bit              not null constraint DF_Entities_IsTranslucent default 0
);

create table EntityLife (
    EntityId         uniqueidentifier not null constraint PK_EntityLife primary key
                                               constraint FK_EntityLife_Entities references Entities (Id),
    MaxHitPoints     bigint           not null,
    CurrentHitPoints bigint           not null
);

create table EntityCharge (
    EntityId      uniqueidentifier not null constraint PK_EntityCharge primary key
                                            constraint FK_EntityCharge_Entities references Entities (Id),
    MaxCharge     bigint           not null,
    CurrentCharge bigint           not null
);