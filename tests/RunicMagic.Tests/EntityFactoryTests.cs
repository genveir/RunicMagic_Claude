using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RunicMagic.Controller;
using RunicMagic.Database;
using RunicMagic.World;
using Xunit;

namespace RunicMagic.Tests;

public class EntityFactoryTests
{
    private static readonly ILogger<EntityFactory> Logger = new Mock<ILogger<EntityFactory>>().Object;

    private static EntityFactory Factory(WorldModel world) => new(world, Logger);

    private static EntityData CreatureData(int maxHp, int currentHp) => new(
        Id: Guid.NewGuid(),
        TypeId: (int)EntityType.Creature,
        Label: "creature",
        X: 0, Y: 0, Width: 10, Height: 10,
        HasAgency: false,
        Weight: 0,
        MaxHitPoints: maxHp,
        CurrentHitPoints: currentHp);

    private static EntityData ManaSourceData(int maxCharge, int currentCharge) => new(
        Id: Guid.NewGuid(),
        TypeId: (int)EntityType.ManaSource,
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

        var drawn = entity.Reservoir!(50);

        drawn.Should().Be(50);
        entity.Life!.CurrentHitPoints.Should().Be(50);
    }

    [Fact]
    public void Creature_Reservoir_CapsAtCurrentHp()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(CreatureData(maxHp: 100, currentHp: 30));
        world.Add(entity);

        var drawn = entity.Reservoir!(50);

        drawn.Should().Be(30);
        entity.Life!.CurrentHitPoints.Should().Be(0);
    }

    [Fact]
    public void Creature_Reservoir_DrawsZeroWhenDead()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(CreatureData(maxHp: 100, currentHp: 0));
        world.Add(entity);

        var drawn = entity.Reservoir!(50);

        drawn.Should().Be(0);
    }

    [Fact]
    public void Creature_Reservoir_DrawsZeroAndLogsWarning_WhenNoLifeCapability()
    {
        var world = new WorldModel();
        var logger = new Mock<ILogger<EntityFactory>>();
        var entity = new EntityFactory(world, logger.Object).Create(new EntityData(
            Guid.NewGuid(), (int)EntityType.Creature, "lifeless creature",
            X: 0, Y: 0, Width: 10, Height: 10, HasAgency: false, Weight: 0));
        world.Add(entity);

        var drawn = entity.Reservoir!(50);

        drawn.Should().Be(0);
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

        var drawn = entity.Reservoir!(75);

        drawn.Should().Be(75);
        entity.Charge!.CurrentCharge.Should().Be(125);
    }

    [Fact]
    public void ManaSource_Reservoir_CapsAtCurrentCharge()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(ManaSourceData(maxCharge: 200, currentCharge: 40));
        world.Add(entity);

        var drawn = entity.Reservoir!(100);

        drawn.Should().Be(40);
        entity.Charge!.CurrentCharge.Should().Be(0);
    }

    [Fact]
    public void ManaSource_Reservoir_DrawsZeroAndLogsWarning_WhenNoChargeCapability()
    {
        var world = new WorldModel();
        var logger = new Mock<ILogger<EntityFactory>>();
        var entity = new EntityFactory(world, logger.Object).Create(new EntityData(
            Guid.NewGuid(), (int)EntityType.ManaSource, "uncharged source",
            X: 0, Y: 0, Width: 10, Height: 10, HasAgency: false, Weight: 0));
        world.Add(entity);

        var drawn = entity.Reservoir!(50);

        drawn.Should().Be(0);
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
    public void Creature_Scope_ReturnsTouchingEntities()
    {
        var world = new WorldModel();
        var caster = Factory(world).Create(CreatureData(maxHp: 100, currentHp: 100) with { X = 0, Y = 0 });
        var nearby = Factory(world).Create(new EntityData(Guid.NewGuid(), (int)EntityType.Object, "nearby",
            X: 10, Y: 0, Width: 10, Height: 10, HasAgency: false, Weight: 0));
        world.Add(caster);
        world.Add(nearby);

        var scope = caster.Scope!();

        scope.Should().Contain(nearby);
    }

    [Fact]
    public void Creature_Scope_DoesNotReturnDistantEntities()
    {
        var world = new WorldModel();
        var caster = Factory(world).Create(CreatureData(maxHp: 100, currentHp: 100) with { X = 0, Y = 0 });
        var distant = Factory(world).Create(new EntityData(Guid.NewGuid(), (int)EntityType.Object, "distant",
            X: 500, Y: 500, Width: 10, Height: 10, HasAgency: false, Weight: 0));
        world.Add(caster);
        world.Add(distant);

        var scope = caster.Scope!();

        scope.Should().NotContain(distant);
    }

    // ── Object ────────────────────────────────────────────────────────────────

    [Fact]
    public void Object_HasNoReservoir()
    {
        var world = new WorldModel();
        var entity = Factory(world).Create(new EntityData(
            Guid.NewGuid(), (int)EntityType.Object, "rock",
            X: 0, Y: 0, Width: 5, Height: 5, HasAgency: false, Weight: 0));

        entity.Reservoir.Should().BeNull();
        entity.Scope.Should().BeNull();
    }
}
