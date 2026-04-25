using FluentAssertions;
using RunicMagic.Tests.Builders;
using RunicMagic.World;
using RunicMagic.World.Services;
using Xunit;

namespace RunicMagic.Tests;

public class DamageServiceTests
{
    private static Entity MakeEntity(long maxIntegrity, long currentIntegrity, long? maxHp = null, long? currentHp = null)
    {
        var builder = new EntityBuilder().WithStructuralIntegrity(maxIntegrity, currentIntegrity);
        if (maxHp.HasValue && currentHp.HasValue) builder.WithLife(maxHp.Value, currentHp.Value);
        return builder.Build();
    }

    private static WorldModel MakeWorldWithEntity(Entity entity)
    {
        var world = new WorldModel();
        world.Add(entity);
        return world;
    }

    [Fact]
    public void Damage_ReducesCurrentIntegrity()
    {
        var entity = MakeEntity(maxIntegrity: 1000, currentIntegrity: 1000);
        var world = MakeWorldWithEntity(entity);

        DamageService.Damage(entity, 300, world);

        entity.StructuralIntegrity.CurrentIntegrity.Should().Be(700);
    }

    [Fact]
    public void Damage_ReturnsActualDamageDealt()
    {
        var entity = MakeEntity(maxIntegrity: 1000, currentIntegrity: 1000);
        var world = MakeWorldWithEntity(entity);

        var dealt = DamageService.Damage(entity, 400, world);

        dealt.Should().Be(400);
    }

    [Fact]
    public void Damage_ClampsAtZero_WhenAmountExceedsCurrentIntegrity()
    {
        var entity = MakeEntity(maxIntegrity: 1000, currentIntegrity: 200);
        var world = MakeWorldWithEntity(entity);

        var dealt = DamageService.Damage(entity, 500, world);

        dealt.Should().Be(200);
        entity.StructuralIntegrity.CurrentIntegrity.Should().Be(0);
    }

    [Fact]
    public void Damage_CapsLife_WhenLifeExceedsNewCurrentIntegrity()
    {
        var entity = MakeEntity(maxIntegrity: 1000, currentIntegrity: 1000, maxHp: 1000, currentHp: 800);
        var world = MakeWorldWithEntity(entity);

        DamageService.Damage(entity, 300, world);

        entity.StructuralIntegrity.CurrentIntegrity.Should().Be(700);
        entity.Life!.CurrentHitPoints.Should().Be(700);
    }

    [Fact]
    public void Damage_DoesNotTouchLife_WhenLifeIsBelowNewCurrentIntegrity()
    {
        var entity = MakeEntity(maxIntegrity: 1000, currentIntegrity: 1000, maxHp: 1000, currentHp: 400);
        var world = MakeWorldWithEntity(entity);

        DamageService.Damage(entity, 300, world);

        entity.StructuralIntegrity.CurrentIntegrity.Should().Be(700);
        entity.Life!.CurrentHitPoints.Should().Be(400);
    }

    [Fact]
    public void Damage_WorksWithNoLife()
    {
        var entity = MakeEntity(maxIntegrity: 1000, currentIntegrity: 1000);
        var world = MakeWorldWithEntity(entity);

        var act = () => DamageService.Damage(entity, 500, world);

        act.Should().NotThrow();
        entity.StructuralIntegrity.CurrentIntegrity.Should().Be(500);
    }

    [Fact]
    public void Damage_CapsLifeToZero_WhenIntegrityReachesZero()
    {
        var entity = MakeEntity(maxIntegrity: 1000, currentIntegrity: 100, maxHp: 1000, currentHp: 600);
        var world = MakeWorldWithEntity(entity);

        DamageService.Damage(entity, 200, world);

        entity.StructuralIntegrity.CurrentIntegrity.Should().Be(0);
        entity.Life!.CurrentHitPoints.Should().Be(0);
    }

    [Fact]
    public void Damage_RemovesEntityFromWorld_WhenIntegrityReachesZero()
    {
        var entity = MakeEntity(maxIntegrity: 1000, currentIntegrity: 100);
        var world = MakeWorldWithEntity(entity);

        DamageService.Damage(entity, 200, world);

        world.GetAll().Should().NotContain(entity);
    }
}
