using FluentAssertions;
using RunicMagic.Controller.Services;
using RunicMagic.Tests.Builders;
using RunicMagic.Tests.Execution;
using RunicMagic.World;
using RunicMagic.World.Capabilities;
using RunicMagic.World.Execution;
using RunicMagic.World.Motion;
using Xunit;

namespace RunicMagic.Tests.Motion;

public class WorldModel_TickMotionTests
{
    [Fact]
    public void TickMotion_AdvancesEffect_EachCall()
    {
        var entity = new EntityBuilder().WithLocation(x: 0, y: 0).WithWeight(1).Build();
        var world = new WorldModel();
        var context = TestFixtures.MakeContext(world: world);

        var effect = new LinearMotionEffect(
            context: context,
            entities: new FixedEntitySet(entity),
            origin: new FixedLocation(-1000, 0),
            perTickDistance: 10,
            perTickCost: 0,
            totalDistanceMm: 560,
            isAway: true,
            effectName: "VUN"
        );
        world.AddMotionEffect(effect);

        world.TickMotion(new EventTracker());
        entity.Location.X.Should().BeApproximately(10, 0.001);

        world.TickMotion(new EventTracker());
        entity.Location.X.Should().BeApproximately(20, 0.001);
    }

    [Fact]
    public void TickMotion_RemovesEffect_WhenComplete()
    {
        var entity = new EntityBuilder().WithLocation(x: 0, y: 0).WithWeight(1).Build();
        var world = new WorldModel();
        var context = TestFixtures.MakeContext(world: world);

        var effect = new LinearMotionEffect(
            context: context,
            entities: new FixedEntitySet(entity),
            origin: new FixedLocation(-1000, 0),
            perTickDistance: 10,
            perTickCost: 0,
            totalDistanceMm: 560,
            isAway: true,
            effectName: "VUN"
        );
        world.AddMotionEffect(effect);

        for (var i = 0; i < 56; i++) world.TickMotion(new EventTracker());

        // After completion, further ticks should not advance the entity
        var xAfterCompletion = entity.Location.X;
        world.TickMotion(new EventTracker());
        entity.Location.X.Should().Be(xAfterCompletion);
    }

    [Fact]
    public void TickMotion_RemovesEffect_OnPowerFailure()
    {
        var casterEntity = new EntityBuilder()
            .WithReservoir(draw: amount => new ReservoirDraw(0, false))
            .Build();
        var caster = new EntitySet([casterEntity]);

        var entity = new EntityBuilder().WithLocation(x: 0, y: 0).WithWeight(1000).Build();
        var world = new WorldModel();
        var context = TestFixtures.MakeContext(caster: caster, world: world);

        var effect = new LinearMotionEffect(
            context: context,
            entities: new FixedEntitySet(entity),
            origin: new FixedLocation(-1000, 0),
            perTickDistance: 10,
            perTickCost: 1,
            totalDistanceMm: 560,
            isAway: true,
            effectName: "VUN"
        );
        world.AddMotionEffect(effect);

        world.TickMotion(new EventTracker()); // fails immediately

        // Effect removed — a second tick should produce no further events
        var secondResult = new EventTracker();
        world.TickMotion(secondResult);
        secondResult.WorldEvents.Should().BeEmpty();
        entity.Location.X.Should().Be(0);
    }

    [Fact]
    public void TickMotion_ReturnsEvents_FromAllActiveEffects()
    {
        var entity1 = new EntityBuilder().WithLocation(x: 0, y: 0).WithWeight(1).Build();
        var entity2 = new EntityBuilder().WithLocation(x: 0, y: 0).WithWeight(1).Build();
        var world = new WorldModel();
        var context = TestFixtures.MakeContext(world: world);

        // Two separate effects — each completes in 56 ticks, emitting one EntityPushedEvent
        var effect1 = new LinearMotionEffect(
            context: context,
            entities: new FixedEntitySet(entity1),
            origin: new FixedLocation(-1000, 0),
            perTickDistance: 10,
            perTickCost: 0,
            totalDistanceMm: 560,
            isAway: true,
            effectName: "VUN"
        );
        var effect2 = new LinearMotionEffect(
            context: context,
            entities: new FixedEntitySet(entity2),
            origin: new FixedLocation(-1000, 0),
            perTickDistance: 10,
            perTickCost: 0,
            totalDistanceMm: 560,
            isAway: true,
            effectName: "VUN"
        );
        world.AddMotionEffect(effect1);
        world.AddMotionEffect(effect2);

        EventTracker finalResult = new EventTracker();
        for (var i = 0; i < 56; i++)
        {
            finalResult = new EventTracker();
            world.TickMotion(finalResult);
        }

        finalResult.WorldEvents.OfType<EntityPushedEvent>().Should().HaveCount(2);
    }

    [Fact]
    public void TickMotion_ReturnsEmptyResult_WhenNoEffectsActive()
    {
        var world = new WorldModel();

        var result = new EventTracker();
        world.TickMotion(result);

        result.WorldEvents.Should().BeEmpty();
    }
}
