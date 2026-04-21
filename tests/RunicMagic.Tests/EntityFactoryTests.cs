using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RunicMagic.Controller;
using RunicMagic.Database;
using RunicMagic.World;
using RunicMagic.World.Runes.EffectRunes;
using Xunit;

namespace RunicMagic.Tests;

public class EntityFactoryTests
{
    private static readonly ILogger<EntityFactory> Logger = new Mock<ILogger<EntityFactory>>().Object;

    private static EntityFactory Factory(WorldModel world) => new(world, Logger);

    private static EntityData CreatureData(long maxHp, long currentHp) => new(
        Id: Guid.NewGuid(),
        TypeId: (long)EntityType.Creature,
        Label: "creature",
        X: 0, Y: 0, Width: 10, Height: 10,
        HasAgency: false,
        Weight: 0,
        MaxHitPoints: maxHp,
        CurrentHitPoints: currentHp);

    private static EntityData ManaSourceData(long maxCharge, long currentCharge) => new(
        Id: Guid.NewGuid(),
        TypeId: (long)EntityType.ManaSource,
        Label: "mana source",
        X: 0, Y: 0, Width: 10, Height: 10,
        HasAgency: false,
        Weight: 0,
        MaxCharge: maxCharge,
        CurrentCharge: currentCharge);

    // ── Creature reservoir ────────────────────────────────────────────────────

    [Fact]
    public void Creature_Reservoir_DrawsFromLife()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(CreatureData(maxHp: 100, currentHp: 100));
        world.Add(entity);

        var draw = entity.Reservoir!(50);

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

        var draw = entity.Reservoir!(50);

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

        var draw = entity.Reservoir!(50);

        draw.Amount.Should().Be(0);
        draw.IsDrained.Should().BeFalse();
    }

    [Fact]
    public void Creature_Reservoir_DrawsZeroAndLogsWarning_WhenNoLifeCapability()
    {
        var world = new WorldModel();
        var logger = new Mock<ILogger<EntityFactory>>();
        var entity = new EntityFactory(world, logger.Object).Create(new EntityData(
            Guid.NewGuid(), (long)EntityType.Creature, "lifeless creature",
            X: 0, Y: 0, Width: 10, Height: 10, HasAgency: false, Weight: 0));
        world.Add(entity);

        var draw = entity.Reservoir!(50);

        draw.Amount.Should().Be(0);
        logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // ── ManaSource reservoir ──────────────────────────────────────────────────

    [Fact]
    public void ManaSource_Reservoir_DrawsFromCharge()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(ManaSourceData(maxCharge: 200, currentCharge: 200));
        world.Add(entity);

        var draw = entity.Reservoir!(75);

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

        var draw = entity.Reservoir!(100);

        draw.Amount.Should().Be(40);
        draw.IsDrained.Should().BeTrue();
        entity.Charge!.CurrentCharge.Should().Be(0);
    }

    [Fact]
    public void ManaSource_Reservoir_DrawsZeroAndLogsWarning_WhenNoChargeCapability()
    {
        var world = new WorldModel();
        var logger = new Mock<ILogger<EntityFactory>>();
        var entity = new EntityFactory(world, logger.Object).Create(new EntityData(
            Guid.NewGuid(), (long)EntityType.ManaSource, "uncharged source",
            X: 0, Y: 0, Width: 10, Height: 10, HasAgency: false, Weight: 0));
        world.Add(entity);

        var draw = entity.Reservoir!(50);

        draw.Amount.Should().Be(0);
        logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // ── Creature scope ────────────────────────────────────────────────────────

    [Fact]
    public void Creature_Scope_ReturnsEntityWithin500mm()
    {
        var world = new WorldModel();
        // Caster centre at (0,0); nearby bbox x:[495,505],y:[-5,5] → nearest point (495,0) → gap=495 < 500
        var caster = Factory(world).Create(CreatureData(maxHp: 100, currentHp: 100) with { X = 0, Y = 0, Width = 10, Height = 10 });
        var nearby = Factory(world).Create(new EntityData(Guid.NewGuid(), (long)EntityType.Object, "nearby",
            X: 500, Y: 0, Width: 10, Height: 10, HasAgency: false, Weight: 0));
        world.Add(caster);
        world.Add(nearby);

        var scope = caster.Scope!();

        scope.Should().Contain(nearby);
    }

    [Fact]
    public void Creature_Scope_DoesNotReturnEntityBeyond500mm()
    {
        var world = new WorldModel();
        // Caster centre at (0,0); distant bbox x:[1005,1015],y:[-5,5] → nearest point (1005,0) → gap=1005 > 500
        var caster = Factory(world).Create(CreatureData(maxHp: 100, currentHp: 100) with { X = 0, Y = 0, Width = 10, Height = 10 });
        var distant = Factory(world).Create(new EntityData(Guid.NewGuid(), (long)EntityType.Object, "distant",
            X: 1010, Y: 0, Width: 10, Height: 10, HasAgency: false, Weight: 0));
        world.Add(caster);
        world.Add(distant);

        var scope = caster.Scope!();

        scope.Should().NotContain(distant);
    }

    // ── MaxReservoir ──────────────────────────────────────────────────────────

    [Fact]
    public void Creature_MaxReservoir_ReturnsMaxHitPoints()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(CreatureData(maxHp: 500, currentHp: 200));
        world.Add(entity);

        var max = entity.MaxReservoir!();

        max.Should().Be(500);
    }

    [Fact]
    public void Creature_MaxReservoir_IsNull_WhenNoLifeCapability()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(new EntityData(
            Guid.NewGuid(), (long)EntityType.Creature, "lifeless",
            X: 0, Y: 0, Width: 10, Height: 10, HasAgency: false, Weight: 0));

        entity.MaxReservoir.Should().BeNull();
    }

    [Fact]
    public void ManaSource_MaxReservoir_ReturnsMaxCharge()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(ManaSourceData(maxCharge: 800, currentCharge: 100));
        world.Add(entity);

        var max = entity.MaxReservoir!();

        max.Should().Be(800);
    }

    [Fact]
    public void ManaSource_MaxReservoir_IsNull_WhenNoChargeCapability()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(new EntityData(
            Guid.NewGuid(), (long)EntityType.ManaSource, "uncharged",
            X: 0, Y: 0, Width: 10, Height: 10, HasAgency: false, Weight: 0));

        entity.MaxReservoir.Should().BeNull();
    }

    // ── Object ────────────────────────────────────────────────────────────────

    [Fact]
    public void Object_HasNoReservoir()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(new EntityData(
            Guid.NewGuid(), (long)EntityType.Object, "rock",
            X: 0, Y: 0, Width: 5, Height: 5, HasAgency: false, Weight: 0));

        entity.Reservoir.Should().BeNull();
    }

    [Fact]
    public void Object_HasNoMaxReservoir()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(new EntityData(
            Guid.NewGuid(), (long)EntityType.Object, "rock",
            X: 0, Y: 0, Width: 5, Height: 5, HasAgency: false, Weight: 0));

        entity.MaxReservoir.Should().BeNull();
    }

    // ── Inscriptions ──────────────────────────────────────────────────────────

    [Fact]
    public void ParsedInscriptions_EmptyArray_WhenNoInscriptionTexts()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(new EntityData(
            Guid.NewGuid(), (long)EntityType.Object, "rock",
            X: 0, Y: 0, Width: 5, Height: 5, HasAgency: false, Weight: 0));

        entity.ParsedInscriptions.Should().BeEmpty();
    }

    [Fact]
    public void ParsedInscriptions_ContainsParsedStatement_WhenValidInscription()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(new EntityData(
            Guid.NewGuid(), (long)EntityType.Object, "rock",
            X: 0, Y: 0, Width: 5, Height: 5, HasAgency: false, Weight: 0,
            InscriptionTexts: ["VUN A HET"]));

        entity.ParsedInscriptions.Should().HaveCount(1);
        entity.ParsedInscriptions[0].Should().BeOfType<VUN>();
    }

    [Fact]
    public void ParsedInscriptions_SilentlyDropsInvalidInscription()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(new EntityData(
            Guid.NewGuid(), (long)EntityType.Object, "rock",
            X: 0, Y: 0, Width: 5, Height: 5, HasAgency: false, Weight: 0,
            InscriptionTexts: ["NOTARUNE"]));

        entity.ParsedInscriptions.Should().BeEmpty();
    }

    [Fact]
    public void ParsedInscriptions_SupportsMultipleInscriptions()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(new EntityData(
            Guid.NewGuid(), (long)EntityType.Object, "rock",
            X: 0, Y: 0, Width: 5, Height: 5, HasAgency: false, Weight: 0,
            InscriptionTexts: ["VUN A HET", "VAR A HET"]));

        entity.ParsedInscriptions.Should().HaveCount(2);
    }

    [Fact]
    public void ParsedInscriptions_SkipsInvalidAndKeepsValidInscriptions()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(new EntityData(
            Guid.NewGuid(), (long)EntityType.Object, "rock",
            X: 0, Y: 0, Width: 5, Height: 5, HasAgency: false, Weight: 0,
            InscriptionTexts: ["NOTARUNE", "VUN A HET"]));

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
        var entity = Factory(world).Create(new EntityData(
            Guid.NewGuid(), (long)EntityType.Object, "rock",
            X: 0, Y: 0, Width: 5, Height: 5, HasAgency: false, Weight: 0));

        entity.RawInscriptions.Should().BeEmpty();
    }

    [Fact]
    public void RawInscriptions_ContainsOriginalText_WhenValidInscription()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(new EntityData(
            Guid.NewGuid(), (long)EntityType.Object, "rock",
            X: 0, Y: 0, Width: 5, Height: 5, HasAgency: false, Weight: 0,
            InscriptionTexts: ["VUN A HET"]));

        entity.RawInscriptions.Should().Equal("VUN A HET");
    }

    [Fact]
    public void RawInscriptions_ContainsOriginalText_WhenInvalidInscription()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(new EntityData(
            Guid.NewGuid(), (long)EntityType.Object, "rock",
            X: 0, Y: 0, Width: 5, Height: 5, HasAgency: false, Weight: 0,
            InscriptionTexts: ["NOTARUNE"]));

        entity.RawInscriptions.Should().Equal("NOTARUNE");
    }

    [Fact]
    public void RawInscriptions_ContainsAllTexts_WhenMixedValidAndInvalid()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(new EntityData(
            Guid.NewGuid(), (long)EntityType.Object, "rock",
            X: 0, Y: 0, Width: 5, Height: 5, HasAgency: false, Weight: 0,
            InscriptionTexts: ["NOTARUNE", "VUN A HET"]));

        entity.RawInscriptions.Should().Equal("NOTARUNE", "VUN A HET");
        entity.ParsedInscriptions.Should().HaveCount(1);
    }
}
