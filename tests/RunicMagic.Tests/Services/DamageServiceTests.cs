using FluentAssertions;
using RunicMagic.Tests.Builders;
using RunicMagic.Tests.Execution;
using RunicMagic.World;
using RunicMagic.World.Execution;
using RunicMagic.World.Services;
using Xunit;

namespace RunicMagic.Tests.Services;

public class DamageServiceTests
{
    [Fact]
    public void Damage_EntitySet_DistributesWithCeilingRounding()
    {
        var entity1 = new EntityBuilder().WithStructuralIntegrity(1000, 1000).Build();
        var entity2 = new EntityBuilder().WithStructuralIntegrity(1000, 1000).Build();
        var entity3 = new EntityBuilder().WithStructuralIntegrity(1000, 1000).Build();

        var set = new EntitySet([entity1, entity2, entity3]);
        DamageService.Damage(set, 10, TestFixtures.MakeContext());

        // ceil(10/3) = 4 per entity
        entity1.StructuralIntegrity.CurrentIntegrity.Should().Be(996);
        entity2.StructuralIntegrity.CurrentIntegrity.Should().Be(996);
        entity3.StructuralIntegrity.CurrentIntegrity.Should().Be(996);
    }

    [Fact]
    public void Damage_EntitySet_EmptySet_DoesNothing()
    {
        var set = new EntitySet([]);
        var act = () => DamageService.Damage(set, 100, TestFixtures.MakeContext());
        act.Should().NotThrow();
    }

    [Fact]
    public void Damage_EntitySet_EvenAmount_DistributesExactly()
    {
        var entity1 = new EntityBuilder().WithStructuralIntegrity(1000, 1000).Build();
        var entity2 = new EntityBuilder().WithStructuralIntegrity(1000, 1000).Build();

        var set = new EntitySet([entity1, entity2]);
        DamageService.Damage(set, 20, TestFixtures.MakeContext());

        entity1.StructuralIntegrity.CurrentIntegrity.Should().Be(990);
        entity2.StructuralIntegrity.CurrentIntegrity.Should().Be(990);
    }

    [Fact]
    public void Damage_EntitySet_EmitsEntityDamagedEvent_PerEntity()
    {
        var entity1 = new EntityBuilder().WithStructuralIntegrity(1000, 1000).Build();
        var entity2 = new EntityBuilder().WithStructuralIntegrity(1000, 1000).Build();

        var set = new EntitySet([entity1, entity2]);
        var result = new SpellResult();
        DamageService.Damage(set, 20, TestFixtures.MakeContext(result: result));

        result.Events.OfType<EntityDamagedEvent>().Should().HaveCount(2);
    }

    [Fact]
    public void Damage_EntitySet_EmitsEntityDisintegratedEvent_WhenEntityDies()
    {
        var entity = new EntityBuilder().WithStructuralIntegrity(100, 10).Build();

        var set = new EntitySet([entity]);
        var result = new SpellResult();
        DamageService.Damage(set, 100, TestFixtures.MakeContext(result: result));

        result.Events.OfType<EntityDisintegratedEvent>().Should().ContainSingle()
            .Which.Entity.Should().Be(entity);
    }

    [Fact]
    public void Damage_EntitySet_ReturnsActualAmountDealt()
    {
        var entity = new EntityBuilder().WithStructuralIntegrity(100, 10).Build();

        var set = new EntitySet([entity]);
        var totalDealt = DamageService.Damage(set, 100, TestFixtures.MakeContext());

        totalDealt.Should().Be(10);
    }

    [Fact]
    public void Damage_EntitySet_SpillsToSurvivorsWhenEntityDies()
    {
        var fragile = new EntityBuilder().WithStructuralIntegrity(1000, 5).Build();
        var sturdy = new EntityBuilder().WithStructuralIntegrity(1000, 1000).Build();

        var set = new EntitySet([fragile, sturdy]);
        var result = new SpellResult();
        DamageService.Damage(set, 100, TestFixtures.MakeContext(result: result));

        // Round 1: ceil(100/2) = 50 each. Fragile absorbs 5 (dies), sturdy absorbs 50. Remaining = 45.
        // Round 2: ceil(45/1) = 45. Sturdy absorbs 45. Remaining = 0.
        sturdy.StructuralIntegrity.CurrentIntegrity.Should().Be(905);
        result.Events.OfType<EntityDisintegratedEvent>().Should().ContainSingle();
    }

    [Fact]
    public void Damage_EntitySet_StopsWhenAllEntitiesDie()
    {
        var entity1 = new EntityBuilder().WithStructuralIntegrity(100, 10).Build();
        var entity2 = new EntityBuilder().WithStructuralIntegrity(100, 10).Build();

        var set = new EntitySet([entity1, entity2]);
        var result = new SpellResult();
        var totalDealt = DamageService.Damage(set, 1000, TestFixtures.MakeContext(result: result));

        totalDealt.Should().Be(20);
        result.Events.OfType<EntityDisintegratedEvent>().Should().HaveCount(2);
    }

    [Fact]
    public void Damage_EntitySet_ReturnsPartialAmountWhenCapacityInsufficient()
    {
        var entity1 = new EntityBuilder().WithStructuralIntegrity(100, 30).Build();
        var entity2 = new EntityBuilder().WithStructuralIntegrity(100, 30).Build();

        var set = new EntitySet([entity1, entity2]);
        var totalDealt = DamageService.Damage(set, 1000, TestFixtures.MakeContext());

        totalDealt.Should().Be(60);
    }

    [Fact]
    public void Damage_Entity_ReducesCurrentIntegrity()
    {
        var entity = new EntityBuilder().WithStructuralIntegrity(1000, 1000).Build();

        DamageService.Damage(entity, 300, TestFixtures.MakeContext());

        entity.StructuralIntegrity.CurrentIntegrity.Should().Be(700);
    }

    [Fact]
    public void Damage_Entity_CapsAtCurrentIntegrity()
    {
        var entity = new EntityBuilder().WithStructuralIntegrity(1000, 50).Build();

        var actual = DamageService.Damage(entity, 500, TestFixtures.MakeContext());

        actual.Should().Be(50);
        entity.StructuralIntegrity.CurrentIntegrity.Should().Be(0);
    }

    [Fact]
    public void Damage_Entity_ReturnsActualAmountDealt()
    {
        var entity = new EntityBuilder().WithStructuralIntegrity(1000, 1000).Build();

        var actual = DamageService.Damage(entity, 400, TestFixtures.MakeContext());

        actual.Should().Be(400);
    }

    [Fact]
    public void Damage_Entity_EmitsEntityDamagedEvent()
    {
        var entity = new EntityBuilder().WithStructuralIntegrity(1000, 1000).Build();

        var result = new SpellResult();
        DamageService.Damage(entity, 100, TestFixtures.MakeContext(result: result));

        result.Events.OfType<EntityDamagedEvent>().Should().ContainSingle()
            .Which.Should().Match<EntityDamagedEvent>(e => e.Entity == entity && e.Amount == 100);
    }

    [Fact]
    public void Damage_Entity_EmitsEntityDisintegratedEvent_WhenIntegrityReachesZero()
    {
        var entity = new EntityBuilder().WithStructuralIntegrity(1000, 100).Build();

        var result = new SpellResult();
        DamageService.Damage(entity, 100, TestFixtures.MakeContext(result: result));

        result.Events.OfType<EntityDisintegratedEvent>().Should().ContainSingle()
            .Which.Entity.Should().Be(entity);
    }

    [Fact]
    public void Damage_Entity_DoesNotEmitEntityDamagedEvent_WhenIntegrityReachesZero()
    {
        var entity = new EntityBuilder().WithStructuralIntegrity(1000, 100).Build();

        var result = new SpellResult();
        DamageService.Damage(entity, 100, TestFixtures.MakeContext(result: result));

        result.Events.OfType<EntityDamagedEvent>().Should().BeEmpty();
    }

    [Fact]
    public void Damage_Entity_DoesNotEmitAnyEvent_WhenZeroDamageDealt()
    {
        var entity = new EntityBuilder().WithStructuralIntegrity(1000, 1000).Build();

        var result = new SpellResult();
        DamageService.Damage(entity, 0, TestFixtures.MakeContext(result: result));

        result.Events.Should().BeEmpty();
    }

    [Fact]
    public void Damage_Entity_ClampsHitPointsToIntegrity_WhenLifeExceedsNewIntegrity()
    {
        var entity = new EntityBuilder()
            .WithStructuralIntegrity(1000, 1000)
            .WithLife(max: 500, current: 500)
            .Build();

        DamageService.Damage(entity, 600, TestFixtures.MakeContext());

        entity.Life!.CurrentHitPoints.Should().Be(400);
    }

    [Fact]
    public void Damage_Entity_DoesNotChangeHitPoints_WhenHitPointsAlreadyBelowNewIntegrity()
    {
        var entity = new EntityBuilder()
            .WithStructuralIntegrity(1000, 1000)
            .WithLife(max: 500, current: 100)
            .Build();

        DamageService.Damage(entity, 200, TestFixtures.MakeContext());

        entity.Life!.CurrentHitPoints.Should().Be(100);
    }

    [Fact]
    public void Damage_Entity_RemovesEntityFromWorld_WhenIntegrityReachesZero()
    {
        var world = new WorldModel();
        var entity = new EntityBuilder().WithStructuralIntegrity(1000, 100).Build();
        world.Add(entity);

        DamageService.Damage(entity, 100, TestFixtures.MakeContext(world: world));

        world.Find(entity.Id).Should().BeNull();
    }
}
