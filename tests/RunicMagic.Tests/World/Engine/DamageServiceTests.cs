using RunicMagic.Controller.Services;
using RunicMagic.World;
using RunicMagic.World.Engine;
using RunicMagic.World.Execution;

namespace RunicMagic.Tests.World.Engine;

public class DamageServiceTests
{
    private static (DamageService damageService, WorldModel world, EventTracker tracker) Setup()
    {
        var world = new WorldModel();
        var tracker = new EventTracker();
        var damageService = new DamageService(world);
        return (damageService, world, tracker);
    }

    [Fact]
    public void Damage_EntitySet_DistributesWithCeilingRounding()
    {
        var entity1 = new EntityBuilder().WithStructuralIntegrity(current: 1000, max: 1000).Build();
        var entity2 = new EntityBuilder().WithStructuralIntegrity(current: 1000, max: 1000).Build();
        var entity3 = new EntityBuilder().WithStructuralIntegrity(current: 1000, max: 1000).Build();
        var (damageService, _, tracker) = Setup();

        var set = new EntitySet([entity1, entity2, entity3]);
        damageService.Damage(set, 10, tracker);

        // ceil(10/3) = 4 per entity
        entity1.StructuralIntegrity.CurrentIntegrity.Should().Be(996);
        entity2.StructuralIntegrity.CurrentIntegrity.Should().Be(996);
        entity3.StructuralIntegrity.CurrentIntegrity.Should().Be(996);
    }

    [Fact]
    public void Damage_EntitySet_EmptySet_DoesNothing()
    {
        var (damageService, _, tracker) = Setup();
        var set = new EntitySet([]);
        var act = () => damageService.Damage(set, 100, tracker);
        act.Should().NotThrow();
    }

    [Fact]
    public void Damage_EntitySet_EvenAmount_DistributesExactly()
    {
        var entity1 = new EntityBuilder().WithStructuralIntegrity(current: 1000, max: 1000).Build();
        var entity2 = new EntityBuilder().WithStructuralIntegrity(current: 1000, max: 1000).Build();
        var (damageService, _, tracker) = Setup();

        var set = new EntitySet([entity1, entity2]);
        damageService.Damage(set, 20, tracker);

        entity1.StructuralIntegrity.CurrentIntegrity.Should().Be(990);
        entity2.StructuralIntegrity.CurrentIntegrity.Should().Be(990);
    }

    [Fact]
    public void Damage_EntitySet_EmitsEntityDamagedEvent_PerEntity()
    {
        var entity1 = new EntityBuilder().WithStructuralIntegrity(current: 1000, max: 1000).Build();
        var entity2 = new EntityBuilder().WithStructuralIntegrity(current: 1000, max: 1000).Build();
        var (damageService, _, tracker) = Setup();

        var set = new EntitySet([entity1, entity2]);
        damageService.Damage(set, 20, tracker);

        tracker.WorldEvents.OfType<EntityDamagedEvent>().Should().HaveCount(2);
    }

    [Fact]
    public void Damage_EntitySet_EmitsEntityDisintegratedEvent_WhenEntityDies()
    {
        var entity = new EntityBuilder().WithStructuralIntegrity(current: 10, max: 100).Build();
        var (damageService, _, tracker) = Setup();

        var set = new EntitySet([entity]);
        damageService.Damage(set, 100, tracker);

        tracker.WorldEvents.OfType<EntityDisintegratedEvent>().Should().ContainSingle()
            .Which.Entity.Should().Be(entity);
    }

    [Fact]
    public void Damage_EntitySet_ReturnsActualAmountDealt()
    {
        var entity = new EntityBuilder().WithStructuralIntegrity(current: 10, max: 100).Build();
        var (damageService, _, tracker) = Setup();

        var set = new EntitySet([entity]);
        var totalDealt = damageService.Damage(set, 100, tracker);

        totalDealt.Should().Be(10);
    }

    [Fact]
    public void Damage_EntitySet_SpillsToSurvivorsWhenEntityDies()
    {
        var fragile = new EntityBuilder().WithStructuralIntegrity(current: 5, max: 1000).Build();
        var sturdy = new EntityBuilder().WithStructuralIntegrity(current: 1000, max: 1000).Build();
        var (damageService, _, tracker) = Setup();

        var set = new EntitySet([fragile, sturdy]);
        damageService.Damage(set, 100, tracker);

        // Round 1: ceil(100/2) = 50 each. Fragile absorbs 5 (dies), sturdy absorbs 50. Remaining = 45.
        // Round 2: ceil(45/1) = 45. Sturdy absorbs 45. Remaining = 0.
        sturdy.StructuralIntegrity.CurrentIntegrity.Should().Be(905);
        tracker.WorldEvents.OfType<EntityDisintegratedEvent>().Should().ContainSingle();
    }

    [Fact]
    public void Damage_EntitySet_StopsWhenAllEntitiesDie()
    {
        var entity1 = new EntityBuilder().WithStructuralIntegrity(current: 10, max: 100).Build();
        var entity2 = new EntityBuilder().WithStructuralIntegrity(current: 10, max: 100).Build();
        var (damageService, _, tracker) = Setup();

        var set = new EntitySet([entity1, entity2]);
        var totalDealt = damageService.Damage(set, 1000, tracker);

        totalDealt.Should().Be(20);
        tracker.WorldEvents.OfType<EntityDisintegratedEvent>().Should().HaveCount(2);
    }

    [Fact]
    public void Damage_EntitySet_ReturnsPartialAmountWhenCapacityInsufficient()
    {
        var entity1 = new EntityBuilder().WithStructuralIntegrity(current: 30, max: 100).Build();
        var entity2 = new EntityBuilder().WithStructuralIntegrity(current: 30, max: 100).Build();
        var (damageService, _, tracker) = Setup();

        var set = new EntitySet([entity1, entity2]);
        var totalDealt = damageService.Damage(set, 1000, tracker);

        totalDealt.Should().Be(60);
    }

    [Fact]
    public void Damage_Entity_ReducesCurrentIntegrity()
    {
        var entity = new EntityBuilder().WithStructuralIntegrity(current: 1000, max: 1000).Build();
        var (damageService, _, tracker) = Setup();

        damageService.Damage(entity, 300, tracker);

        entity.StructuralIntegrity.CurrentIntegrity.Should().Be(700);
    }

    [Fact]
    public void Damage_Entity_CapsAtCurrentIntegrity()
    {
        var entity = new EntityBuilder().WithStructuralIntegrity(current: 50, max: 1000).Build();
        var (damageService, _, tracker) = Setup();

        var actual = damageService.Damage(entity, 500, tracker);

        actual.Should().Be(50);
        entity.StructuralIntegrity.CurrentIntegrity.Should().Be(0);
    }

    [Fact]
    public void Damage_Entity_ReturnsActualAmountDealt()
    {
        var entity = new EntityBuilder().WithStructuralIntegrity(current: 1000, max: 1000).Build();
        var (damageService, _, tracker) = Setup();

        var actual = damageService.Damage(entity, 400, tracker);

        actual.Should().Be(400);
    }

    [Fact]
    public void Damage_Entity_EmitsEntityDamagedEvent()
    {
        var entity = new EntityBuilder().WithStructuralIntegrity(current: 1000, max: 1000).Build();
        var (damageService, _, tracker) = Setup();

        damageService.Damage(entity, 100, tracker);

        tracker.WorldEvents.OfType<EntityDamagedEvent>().Should().ContainSingle()
            .Which.Should().Match<EntityDamagedEvent>(e => e.Entity == entity && e.Amount == 100);
    }

    [Fact]
    public void Damage_Entity_EmitsEntityDisintegratedEvent_WhenIntegrityReachesZero()
    {
        var entity = new EntityBuilder().WithStructuralIntegrity(current: 100, max: 1000).Build();
        var (damageService, _, tracker) = Setup();

        damageService.Damage(entity, 100, tracker);

        tracker.WorldEvents.OfType<EntityDisintegratedEvent>().Should().ContainSingle()
            .Which.Entity.Should().Be(entity);
    }

    [Fact]
    public void Damage_Entity_DoesNotEmitEntityDamagedEvent_WhenIntegrityReachesZero()
    {
        var entity = new EntityBuilder().WithStructuralIntegrity(current: 100, max: 1000).Build();
        var (damageService, _, tracker) = Setup();

        damageService.Damage(entity, 100, tracker);

        tracker.WorldEvents.OfType<EntityDamagedEvent>().Should().BeEmpty();
    }

    [Fact]
    public void Damage_Entity_DoesNotEmitAnyEvent_WhenZeroDamageDealt()
    {
        var entity = new EntityBuilder().WithStructuralIntegrity(current: 1000, max: 1000).Build();
        var (damageService, _, tracker) = Setup();

        damageService.Damage(entity, 0, tracker);

        tracker.WorldEvents.Should().BeEmpty();
    }

    [Fact]
    public void Damage_Entity_ClampsHitPointsToIntegrity_WhenLifeExceedsNewIntegrity()
    {
        var entity = new EntityBuilder()
            .WithStructuralIntegrity(current: 1000, max: 1000)
            .WithLife(max: 500, current: 500)
            .Build();
        var (damageService, _, tracker) = Setup();

        damageService.Damage(entity, 600, tracker);

        entity.Life!.CurrentHitPoints.Should().Be(400);
    }

    [Fact]
    public void Damage_Entity_DoesNotChangeHitPoints_WhenHitPointsAlreadyBelowNewIntegrity()
    {
        var entity = new EntityBuilder()
            .WithStructuralIntegrity(current: 1000, max: 1000)
            .WithLife(max: 500, current: 100)
            .Build();
        var (damageService, _, tracker) = Setup();

        damageService.Damage(entity, 200, tracker);

        entity.Life!.CurrentHitPoints.Should().Be(100);
    }

    [Fact]
    public void Damage_Entity_RemovesEntityFromWorld_WhenIntegrityReachesZero()
    {
        var entity = new EntityBuilder().WithStructuralIntegrity(current: 100, max: 1000).Build();
        var (damageService, world, tracker) = Setup();
        world.Add(entity);

        damageService.Damage(entity, 100, tracker);

        world.Find(entity.Id).Should().BeNull();
    }
}
