using RunicMagic.Controller.Services;
using RunicMagic.World;
using RunicMagic.World.Engine;
using RunicMagic.World.Entities;

namespace RunicMagic.Tests.World.Entities.Capabilities;

public class DamageServiceTests
{
    private static Entity MakeEntity(long maxIntegrity, long currentIntegrity, long? maxHp = null, long? currentHp = null)
    {
        var builder = new EntityBuilder().WithStructuralIntegrity(current: currentIntegrity, max: maxIntegrity);
        if (maxHp.HasValue && currentHp.HasValue) builder.WithLife(current: currentHp.Value, max: maxHp.Value);
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
        var damageService = new DamageService(world);

        damageService.Damage(entity, 300, new EventTracker());

        entity.StructuralIntegrity.CurrentIntegrity.Should().Be(700);
    }

    [Fact]
    public void Damage_ReturnsActualDamageDealt()
    {
        var entity = MakeEntity(maxIntegrity: 1000, currentIntegrity: 1000);
        var world = MakeWorldWithEntity(entity);
        var damageService = new DamageService(world);

        var dealt = damageService.Damage(entity, 400, new EventTracker());

        dealt.Should().Be(400);
    }

    [Fact]
    public void Damage_ClampsAtZero_WhenAmountExceedsCurrentIntegrity()
    {
        var entity = MakeEntity(maxIntegrity: 1000, currentIntegrity: 200);
        var world = MakeWorldWithEntity(entity);
        var damageService = new DamageService(world);

        var dealt = damageService.Damage(entity, 500, new EventTracker());

        dealt.Should().Be(200);
        entity.StructuralIntegrity.CurrentIntegrity.Should().Be(0);
    }

    [Fact]
    public void Damage_CapsLife_WhenLifeExceedsNewCurrentIntegrity()
    {
        var entity = MakeEntity(maxIntegrity: 1000, currentIntegrity: 1000, maxHp: 1000, currentHp: 800);
        var world = MakeWorldWithEntity(entity);
        var damageService = new DamageService(world);

        damageService.Damage(entity, 300, new EventTracker());

        entity.StructuralIntegrity.CurrentIntegrity.Should().Be(700);
        entity.Life!.CurrentHitPoints.Should().Be(700);
    }

    [Fact]
    public void Damage_DoesNotTouchLife_WhenLifeIsBelowNewCurrentIntegrity()
    {
        var entity = MakeEntity(maxIntegrity: 1000, currentIntegrity: 1000, maxHp: 1000, currentHp: 400);
        var world = MakeWorldWithEntity(entity);
        var damageService = new DamageService(world);

        damageService.Damage(entity, 300, new EventTracker());

        entity.StructuralIntegrity.CurrentIntegrity.Should().Be(700);
        entity.Life!.CurrentHitPoints.Should().Be(400);
    }

    [Fact]
    public void Damage_WorksWithNoLife()
    {
        var entity = MakeEntity(maxIntegrity: 1000, currentIntegrity: 1000);
        var world = MakeWorldWithEntity(entity);
        var damageService = new DamageService(world);

        var act = () => damageService.Damage(entity, 500, new EventTracker());

        act.Should().NotThrow();
        entity.StructuralIntegrity.CurrentIntegrity.Should().Be(500);
    }

    [Fact]
    public void Damage_CapsLifeToZero_WhenIntegrityReachesZero()
    {
        var entity = MakeEntity(maxIntegrity: 1000, currentIntegrity: 100, maxHp: 1000, currentHp: 600);
        var world = MakeWorldWithEntity(entity);
        var damageService = new DamageService(world);

        damageService.Damage(entity, 200, new EventTracker());

        entity.StructuralIntegrity.CurrentIntegrity.Should().Be(0);
        entity.Life!.CurrentHitPoints.Should().Be(0);
    }

    [Fact]
    public void Damage_RemovesEntityFromWorld_WhenIntegrityReachesZero()
    {
        var entity = MakeEntity(maxIntegrity: 1000, currentIntegrity: 100);
        var world = MakeWorldWithEntity(entity);
        var damageService = new DamageService(world);

        damageService.Damage(entity, 200, new EventTracker());

        world.GetAll().Should().NotContain(entity);
    }
}
