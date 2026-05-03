using RunicMagic.Controller.Services;
using RunicMagic.World.Entities;
using RunicMagic.World.Entities.Capabilities;
using RunicMagic.World.Execution;
using RunicMagic.World.Motion.Engine;

namespace RunicMagic.Tests.World.Motion.Engine;

public class LinearEngineMotionEffectTests
{
    private static LinearEngineMotionEffect MakePushEffect(
        SpellContext context,
        Entity entity,
        long totalDistance)
    {
        return new LinearEngineMotionEffect(
            context: context,
            toMove: new FixedEntitySet(entity),
            origin: new FixedLocation(0, 0),
            perTickDistance: totalDistance / 56.0,
            totalDistanceMm: totalDistance,
            isAway: true,
            effectName: "VUN"
        );
    }

    [Fact]
    public void TryAdvance_After56Ticks_EntityReachesDestination()
    {
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(0).Build();
        var context = TestFixtures.MakeContext();
        var effect = MakePushEffect(context, entity, totalDistance: 560);

        for (var i = 0; i < 56; i++)
        {
            effect.TryAdvance(new EventTracker());
        }

        entity.Location.X.Should().BeApproximately(1560, 0.001);
        entity.Location.Y.Should().BeApproximately(0, 0.001);
        effect.IsComplete.Should().BeTrue();
    }

    [Fact]
    public void TryAdvance_IsCompleteAfter56Ticks()
    {
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(0).Build();
        var context = TestFixtures.MakeContext();
        var effect = MakePushEffect(context, entity, totalDistance: 100);

        effect.IsComplete.Should().BeFalse();

        for (var i = 0; i < 55; i++) effect.TryAdvance(new EventTracker());
        effect.IsComplete.Should().BeFalse();

        effect.TryAdvance(new EventTracker());
        effect.IsComplete.Should().BeTrue();
    }

    [Fact]
    public void TryAdvance_EmitsEntityPushedEvent_OnLastTick()
    {
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(0).Build();
        var context = TestFixtures.MakeContext();
        var effect = MakePushEffect(context, entity, totalDistance: 100);

        EventTracker lastResult = new EventTracker();
        for (var i = 0; i < 56; i++)
        {
            lastResult = new EventTracker();
            effect.TryAdvance(lastResult);
        }

        lastResult.WorldEvents.OfType<EntityPushedEvent>().Should().ContainSingle()
            .Which.DistanceMm.Should().Be(100);
    }

    [Fact]
    public void TryAdvance_NoEntityPushedEvent_OnIntermediateTicks()
    {
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(0).Build();
        var context = TestFixtures.MakeContext();
        var effect = MakePushEffect(context, entity, totalDistance: 100);

        var intermediateResult = new EventTracker();
        effect.TryAdvance(intermediateResult);

        intermediateResult.WorldEvents.OfType<EntityPushedEvent>().Should().BeEmpty();
    }

    [Fact]
    public void TryAdvance_InsufficientPower_ReturnsFalse()
    {
        // 56000mm × 1000g / 1_000_000 = 56 total cost; 1 per tick
        var casterEntity = new EntityBuilder()
            .WithReservoir(draw: amount => new ReservoirDraw(0, false))
            .Build();
        var caster = new EntitySet([casterEntity]);
        var context = TestFixtures.MakeContext(caster: caster);

        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(1000).Build();
        var effect = new LinearEngineMotionEffect(
            context: context,
            toMove: new FixedEntitySet(entity),
            origin: new FixedLocation(0, 0),
            perTickDistance: 1000,
            totalDistanceMm: 56000,
            isAway: true,
            effectName: "VUN"
        );

        var result = new EventTracker();
        var (advanced, _) = effect.TryAdvance(result);

        advanced.Should().BeFalse();
        result.WorldEvents.OfType<EffectNotFiredEvent>().Should().ContainSingle()
            .Which.Effect.Should().Be("VUN");
    }

    [Fact]
    public void TryAdvance_InsufficientPower_EntityDoesNotMove()
    {
        var casterEntity = new EntityBuilder()
            .WithReservoir(draw: amount => new ReservoirDraw(0, false))
            .Build();
        var caster = new EntitySet([casterEntity]);
        var context = TestFixtures.MakeContext(caster: caster);

        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(1000).Build();
        var effect = new LinearEngineMotionEffect(
            context: context,
            toMove: new FixedEntitySet(entity),
            origin: new FixedLocation(0, 0),
            perTickDistance: 1000,
            totalDistanceMm: 56000,
            isAway: true,
            effectName: "VUN"
        );

        effect.TryAdvance(new EventTracker());

        entity.Location.X.Should().Be(1000);
    }

    [Fact]
    public void TryAdvance_PartialPower_StopsEarly()
    {
        // 1 power per tick, caster has power for exactly 3 ticks
        var ticksDrawn = 0;
        var casterEntity = new EntityBuilder()
            .WithReservoir(draw: amount =>
            {
                if (ticksDrawn < 3)
                {
                    ticksDrawn++;
                    return new ReservoirDraw(amount, false);
                }
                return new ReservoirDraw(0, false);
            })
            .Build();
        var caster = new EntitySet([casterEntity]);
        var context = TestFixtures.MakeContext(caster: caster);

        var entity = new EntityBuilder().WithLocation(x: 0, y: 0).WithWeight(1000).Build();
        var effect = new LinearEngineMotionEffect(
            context: context,
            toMove: new FixedEntitySet(entity),
            origin: new FixedLocation(-1000, 0),
            perTickDistance: 100,
            totalDistanceMm: 5600,
            isAway: true,
            effectName: "VUN"
        );

        for (var i = 0; i < 56; i++) effect.TryAdvance(new EventTracker());

        // 3 successful ticks × 100mm/tick = 300mm total
        entity.Location.X.Should().BeApproximately(300, 0.001);
    }

    [Fact]
    public void TryAdvance_PullMovesEntityTowardOrigin()
    {
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(0).Build();
        var context = TestFixtures.MakeContext();
        var effect = new LinearEngineMotionEffect(
            context: context,
            toMove: new FixedEntitySet(entity),
            origin: new FixedLocation(0, 0),
            perTickDistance: 500 / 56.0,
            totalDistanceMm: 500,
            isAway: false,
            effectName: "VAR"
        );

        for (var i = 0; i < 56; i++) effect.TryAdvance(new EventTracker());

        entity.Location.X.Should().BeApproximately(500, 0.001);
        entity.Location.Y.Should().BeApproximately(0, 0.001);
    }

    [Fact]
    public void TryAdvance_ZeroCost_AlwaysSucceeds()
    {
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(0).Build();
        var context = TestFixtures.MakeContext(); // no power sources
        var effect = new LinearEngineMotionEffect(
            context: context,
            toMove: new FixedEntitySet(entity),
            origin: new FixedLocation(0, 0),
            perTickDistance: 10,
            totalDistanceMm: 560,
            isAway: true,
            effectName: "VUN"
        );

        for (var i = 0; i < 56; i++) effect.TryAdvance(new EventTracker());

        entity.Location.X.Should().BeApproximately(1560, 0.001);
    }

    [Fact]
    public void TryAdvance_ReturnsEntitiesUnderMotion_OnSuccess()
    {
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(0).Build();
        var context = TestFixtures.MakeContext();
        var effect = MakePushEffect(context, entity, totalDistance: 560);

        var (_, entitiesUnderMotion) = effect.TryAdvance(new EventTracker());

        entitiesUnderMotion.Should().ContainSingle().Which.Should().BeSameAs(entity);
    }

    [Fact]
    public void TryAdvance_ReturnsNoEntitiesUnderMotion_OnInsufficientPower()
    {
        var casterEntity = new EntityBuilder()
            .WithReservoir(draw: amount => new ReservoirDraw(0, false))
            .Build();
        var caster = new EntitySet([casterEntity]);
        var context = TestFixtures.MakeContext(caster: caster);

        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(1000).Build();
        var effect = new LinearEngineMotionEffect(
            context: context,
            toMove: new FixedEntitySet(entity),
            origin: new FixedLocation(0, 0),
            perTickDistance: 1000,
            totalDistanceMm: 56000,
            isAway: true,
            effectName: "VUN"
        );

        var (_, entitiesUnderMotion) = effect.TryAdvance(new EventTracker());

        entitiesUnderMotion.Should().BeEmpty();
    }
}
