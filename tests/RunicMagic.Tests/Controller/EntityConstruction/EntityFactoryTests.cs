using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RunicMagic.Controller.EntityConstruction;
using RunicMagic.Database;
using RunicMagic.World;
using RunicMagic.World.Entities;
using RunicMagic.World.Runes.EffectRunes;

namespace RunicMagic.Tests.Controller.EntityConstruction;

public class EntityFactoryTests
{
    private static readonly ILogger<EntityFactory> Logger = new NullLogger<EntityFactory>();

    private static EntityFactory Factory(WorldModel world)
    {
        var engineAPI = EngineAPIBuilder.ForWorldModel(world).Build();
        return new EntityFactory(world, Logger, engineAPI);
    }

    private static EntityData DefaultEntityData()
    {
        var data = new EntityData(
            Id: Guid.NewGuid(),
            TypeId: (long)EntityType.Object,
            Label: "entity",
            X: 0, Y: 0, Width: 10, Height: 10,
            HasAgency: false,
            Weight: 0,
            Strength: 0,
            IsTranslucent: false,
            Facing: 0,
            MaxHitPoints: null,
            CurrentHitPoints: null,
            MaxCharge: null,
            CurrentCharge: null,
            InscriptionTexts: null,
            MaxStructuralIntegrity: 1000,
            CurrentStructuralIntegrity: 1000,
            DragCoefficient: 0.0,
            AngularDragCoefficient: 0.0,
            GroundFrictionCoefficient: 0.0,
            AIData: null,
            LocomotionEfficiency: null);
        return data;
    }

    private static EntityData CreatureData(long maxHp, long currentHp)
    {
        var data = DefaultEntityData() with
        {
            TypeId = (long)EntityType.Creature,
            Label = "creature",
            MaxHitPoints = maxHp,
            CurrentHitPoints = currentHp
        };
        return data;
    }

    private static EntityData ManaSourceData(long maxCharge, long currentCharge)
    {
        var data = DefaultEntityData() with
        {
            TypeId = (long)EntityType.ManaSource,
            Label = "mana source",
            MaxCharge = maxCharge,
            CurrentCharge = currentCharge
        };
        return data;
    }

    private static EntityData MinimalData(long typeId, string label, long width = 10, long height = 10)
    {
        var data = DefaultEntityData() with
        {
            TypeId = typeId,
            Label = label,
            Width = width,
            Height = height
        };
        return data;
    }

    // ── Creature reservoir — Draw ─────────────────────────────────────────────

    [Fact]
    public void Creature_Reservoir_DrawsFromLife()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(CreatureData(maxHp: 100, currentHp: 100));
        world.Add(entity);

        var draw = entity.Reservoir!.Draw(50);

        draw.Amount.Should().Be(50);
        draw.IsDrained.Should().BeFalse();
        entity.Life!.CurrentHitPoints.Should().Be(50);
    }

    [Fact]
    public void Creature_Reservoir_CapsAtCurrentHp()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(CreatureData(maxHp: 100, currentHp: 30));
        world.Add(entity);

        var draw = entity.Reservoir!.Draw(50);

        draw.Amount.Should().Be(30);
        draw.IsDrained.Should().BeTrue();
        entity.Life!.CurrentHitPoints.Should().Be(0);
    }

    [Fact]
    public void Creature_Reservoir_DrawsZeroWhenDead()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(CreatureData(maxHp: 100, currentHp: 0));
        world.Add(entity);

        var draw = entity.Reservoir!.Draw(50);

        draw.Amount.Should().Be(0);
        draw.IsDrained.Should().BeFalse();
    }

    // ── Creature reservoir — Current and Max ──────────────────────────────────

    [Fact]
    public void Creature_Reservoir_Current_ReturnsCurrentHp()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(CreatureData(maxHp: 100, currentHp: 60));
        world.Add(entity);

        var current = entity.Reservoir!.Current();

        current.Should().Be(60);
    }

    [Fact]
    public void Creature_Reservoir_Current_TracksHpChanges()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(CreatureData(maxHp: 100, currentHp: 100));
        world.Add(entity);

        entity.Reservoir!.Draw(40);
        var current = entity.Reservoir!.Current();

        current.Should().Be(60);
    }

    [Fact]
    public void Creature_MaxReservoir_ReturnsMaxHitPoints()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(CreatureData(maxHp: 500, currentHp: 200));
        world.Add(entity);

        var max = entity.Reservoir!.Max();

        max.Should().Be(500);
    }

    // ── Creature reservoir — Fill ─────────────────────────────────────────────

    [Fact]
    public void Creature_Reservoir_Fill_AddsToHp()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(CreatureData(maxHp: 100, currentHp: 40));
        world.Add(entity);

        var fill = entity.Reservoir!.Fill(30);

        fill.Amount.Should().Be(30);
        fill.IsFull.Should().BeFalse();
        entity.Life!.CurrentHitPoints.Should().Be(70);
    }

    [Fact]
    public void Creature_Reservoir_Fill_CapsAtMaxHp()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(CreatureData(maxHp: 100, currentHp: 80));
        world.Add(entity);

        var fill = entity.Reservoir!.Fill(50);

        fill.Amount.Should().Be(20);
        fill.IsFull.Should().BeTrue();
        entity.Life!.CurrentHitPoints.Should().Be(100);
    }

    [Fact]
    public void Creature_Reservoir_Fill_ReturnsZero_WhenAlreadyFull()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(CreatureData(maxHp: 100, currentHp: 100));
        world.Add(entity);

        var fill = entity.Reservoir!.Fill(50);

        fill.Amount.Should().Be(0);
        fill.IsFull.Should().BeTrue();
    }

    // ── Creature — null checks ────────────────────────────────────────────────

    [Fact]
    public void Creature_Reservoir_IsNull_WhenNoLifeCapability()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(MinimalData((long)EntityType.Creature, "lifeless"));

        entity.Reservoir.Should().BeNull();
    }

    // ── ManaSource reservoir — Draw ───────────────────────────────────────────

    [Fact]
    public void ManaSource_Reservoir_DrawsFromCharge()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(ManaSourceData(maxCharge: 200, currentCharge: 200));
        world.Add(entity);

        var draw = entity.Reservoir!.Draw(75);

        draw.Amount.Should().Be(75);
        draw.IsDrained.Should().BeFalse();
        entity.Charge!.CurrentCharge.Should().Be(125);
    }

    [Fact]
    public void ManaSource_Reservoir_CapsAtCurrentCharge()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(ManaSourceData(maxCharge: 200, currentCharge: 40));
        world.Add(entity);

        var draw = entity.Reservoir!.Draw(100);

        draw.Amount.Should().Be(40);
        draw.IsDrained.Should().BeTrue();
        entity.Charge!.CurrentCharge.Should().Be(0);
    }

    // ── ManaSource reservoir — Current and Max ────────────────────────────────

    [Fact]
    public void ManaSource_Reservoir_Current_ReturnsCurrentCharge()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(ManaSourceData(maxCharge: 200, currentCharge: 120));
        world.Add(entity);

        var current = entity.Reservoir!.Current();

        current.Should().Be(120);
    }

    [Fact]
    public void ManaSource_Reservoir_Current_TracksChargeChanges()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(ManaSourceData(maxCharge: 200, currentCharge: 200));
        world.Add(entity);

        entity.Reservoir!.Draw(50);
        var current = entity.Reservoir!.Current();

        current.Should().Be(150);
    }

    [Fact]
    public void ManaSource_Reservoir_ReturnsMaxCharge()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(ManaSourceData(maxCharge: 800, currentCharge: 100));
        world.Add(entity);

        var max = entity.Reservoir!.Max();

        max.Should().Be(800);
    }

    // ── ManaSource reservoir — Fill ───────────────────────────────────────────

    [Fact]
    public void ManaSource_Reservoir_Fill_AddsToCharge()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(ManaSourceData(maxCharge: 200, currentCharge: 50));
        world.Add(entity);

        var fill = entity.Reservoir!.Fill(80);

        fill.Amount.Should().Be(80);
        fill.IsFull.Should().BeFalse();
        entity.Charge!.CurrentCharge.Should().Be(130);
    }

    [Fact]
    public void ManaSource_Reservoir_Fill_CapsAtMaxCharge()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(ManaSourceData(maxCharge: 200, currentCharge: 170));
        world.Add(entity);

        var fill = entity.Reservoir!.Fill(100);

        fill.Amount.Should().Be(30);
        fill.IsFull.Should().BeTrue();
        entity.Charge!.CurrentCharge.Should().Be(200);
    }

    [Fact]
    public void ManaSource_Reservoir_Fill_ReturnsZero_WhenAlreadyFull()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(ManaSourceData(maxCharge: 200, currentCharge: 200));
        world.Add(entity);

        var fill = entity.Reservoir!.Fill(50);

        fill.Amount.Should().Be(0);
        fill.IsFull.Should().BeTrue();
    }

    // ── ManaSource — null checks ──────────────────────────────────────────────

    [Fact]
    public void ManaSource_Reservoir_IsNull_WhenNoChargeCapability()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(MinimalData((long)EntityType.ManaSource, "uncharged", height: 5));

        entity.Reservoir.Should().BeNull();
    }

    // ── AI ────────────────────────────────────────────────────────────────────

    [Fact]
    public void Create_EntityHasAICapability_WithZeroBehaviors()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(MinimalData((long)EntityType.Object, "rock", width: 5, height: 5));

        entity.AI.Should().NotBeNull();
        entity.AI.BehaviorCount.Should().Be(0);
    }

    // ── Object ────────────────────────────────────────────────────────────────

    [Fact]
    public void Object_HasNoReservoir()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(MinimalData((long)EntityType.Object, "rock", width: 5, height: 5));

        entity.Reservoir.Should().BeNull();
    }

    // ── Creature scope ────────────────────────────────────────────────────────

    [Fact]
    public void Creature_Scope_ReturnsEntityWithin500mm()
    {
        var world = new WorldModel();
        // Caster centre at (0,0); nearby bbox x:[495,505],y:[-5,5] → nearest point (495,0) → gap=495 < 500
        var caster = Factory(world).Create(CreatureData(maxHp: 100, currentHp: 100) with { X = 0, Y = 0, Width = 10, Height = 10 });
        var nearby = Factory(world).Create(MinimalData((long)EntityType.Object, "nearby") with { X = 500 });
        world.Add(caster);
        world.Add(nearby);

        var scope = caster.Scope!();

        scope.Entities.Should().Contain(nearby);
    }

    [Fact]
    public void Creature_Scope_DoesNotReturnEntityBeyond500mm()
    {
        var world = new WorldModel();
        // Caster centre at (0,0); distant bbox x:[1005,1015],y:[-5,5] → nearest point (1005,0) → gap=1005 > 500
        var caster = Factory(world).Create(CreatureData(maxHp: 100, currentHp: 100) with { X = 0, Y = 0, Width = 10, Height = 10 });
        var distant = Factory(world).Create(MinimalData((long)EntityType.Object, "distant") with { X = 1010 });
        world.Add(caster);
        world.Add(distant);

        var scope = caster.Scope!();

        scope.Entities.Should().NotContain(distant);
    }

    // ── Inscriptions ──────────────────────────────────────────────────────────

    [Fact]
    public void ParsedInscriptions_EmptyArray_WhenNoInscriptionTexts()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(MinimalData((long)EntityType.Object, "rock", width: 5, height: 5));

        entity.ParsedInscriptions.Should().BeEmpty();
    }

    [Fact]
    public void ParsedInscriptions_ContainsParsedStatement_WhenValidInscription()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(MinimalData((long)EntityType.Object, "rock", width: 5, height: 5) with { InscriptionTexts = ["VUN A HET"] });

        entity.ParsedInscriptions.Should().HaveCount(1);
        entity.ParsedInscriptions[0].Should().BeOfType<VUN>();
    }

    [Fact]
    public void ParsedInscriptions_SilentlyDropsInvalidInscription()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(MinimalData((long)EntityType.Object, "rock", width: 5, height: 5) with { InscriptionTexts = ["NOTARUNE"] });

        entity.ParsedInscriptions.Should().BeEmpty();
    }

    [Fact]
    public void ParsedInscriptions_SupportsMultipleInscriptions()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(MinimalData((long)EntityType.Object, "rock", width: 5, height: 5) with { InscriptionTexts = ["VUN A HET", "VAR A HET"] });

        entity.ParsedInscriptions.Should().HaveCount(2);
    }

    [Fact]
    public void ParsedInscriptions_SkipsInvalidAndKeepsValidInscriptions()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(MinimalData((long)EntityType.Object, "rock", width: 5, height: 5) with { InscriptionTexts = ["NOTARUNE", "VUN A HET"] });

        entity.ParsedInscriptions.Should().HaveCount(1);
        entity.ParsedInscriptions[0].Should().BeOfType<VUN>();
    }

    [Fact]
    public void ParsedInscriptions_WorksOnCreature()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(
            CreatureData(maxHp: 100, currentHp: 100) with
            {
                InscriptionTexts = ["VUN A HET"]
            });
        world.Add(entity);

        entity.ParsedInscriptions.Should().HaveCount(1);
    }

    // ── RawInscriptions ───────────────────────────────────────────────────────

    [Fact]
    public void RawInscriptions_EmptyArray_WhenNoInscriptionTexts()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(MinimalData((long)EntityType.Object, "rock", width: 5, height: 5));

        entity.RawInscriptions.Should().BeEmpty();
    }

    [Fact]
    public void RawInscriptions_ContainsOriginalText_WhenValidInscription()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(MinimalData((long)EntityType.Object, "rock", width: 5, height: 5) with { InscriptionTexts = ["VUN A HET"] });

        entity.RawInscriptions.Should().Equal("VUN A HET");
    }

    [Fact]
    public void RawInscriptions_ContainsOriginalText_WhenInvalidInscription()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(MinimalData((long)EntityType.Object, "rock", width: 5, height: 5) with { InscriptionTexts = ["NOTARUNE"] });

        entity.RawInscriptions.Should().Equal("NOTARUNE");
    }

    [Fact]
    public void RawInscriptions_ContainsAllTexts_WhenMixedValidAndInvalid()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(MinimalData((long)EntityType.Object, "rock", width: 5, height: 5) with { InscriptionTexts = ["NOTARUNE", "VUN A HET"] });

        entity.RawInscriptions.Should().Equal("NOTARUNE", "VUN A HET");
        entity.ParsedInscriptions.Should().HaveCount(1);
    }
}
