using RunicMagic.Controller.Services;
using RunicMagic.World;
using RunicMagic.World.Entities.Capabilities;
using RunicMagic.World.Execution;
using RunicMagic.World.Motion.Engine;

namespace RunicMagic.Tests.World;

public class WorldModel_TickMotionTests
{
    private static (SpellContext context, WorldTicker ticker) MakeContextAndTicker(
        WorldModel world,
        EntitySet? caster = null)
    {
        var engineMotionCollection = new EngineMotionCollection();
        var engineAPI = EngineAPIBuilder.ForWorldModel(world).WithEngineMotionCollection(engineMotionCollection).Build();
        var context = TestFixtures.MakeContext(caster: caster, engineAPI: engineAPI);
        var ticker = WorldTickerBuilder.ForWorldModel(world).WithEngineMotionCollection(engineMotionCollection).Build();
        return (context, ticker);
    }

    [Fact]
    public void TickMotion_AdvancesEffect_EachCall()
    {
        var entity = new EntityBuilder().WithLocation(x: 0, y: 0).WithWeight(0).Build();
        var world = new WorldModel();
        var (context, ticker) = MakeContextAndTicker(world);

        var effect = new LinearEngineMotionEffect(
            context: context,
            toMove: new FixedEntitySet(entity),
            origin: new FixedLocation(-1000, 0),
            totalDistanceMm: Constants.DefaultEffectLength * 10,
            tickCount: Constants.DefaultEffectLength,
            isAway: true,
            effectName: "VUN"
        );
        context.EngineAPI.EngineMotion.AddMotionEffect(effect);

        ticker.HandleTick(new EventTracker(), 0);
        entity.Location.X.Should().BeApproximately(10, 0.001);

        ticker.HandleTick(new EventTracker(), 1);
        entity.Location.X.Should().BeApproximately(20, 0.001);
    }

    [Fact]
    public void TickMotion_RemovesEffect_WhenComplete()
    {
        var entity = new EntityBuilder().WithLocation(x: 0, y: 0).WithWeight(0).Build();
        var world = new WorldModel();
        var (context, ticker) = MakeContextAndTicker(world);

        var effect = new LinearEngineMotionEffect(
            context: context,
            toMove: new FixedEntitySet(entity),
            origin: new FixedLocation(-1000, 0),
            totalDistanceMm: Constants.DefaultEffectLength * 10,
            tickCount: Constants.DefaultEffectLength,
            isAway: true,
            effectName: "VUN"
        );
        context.EngineAPI.EngineMotion.AddMotionEffect(effect);

        for (var i = 0; i < Constants.DefaultEffectLength; i++) ticker.HandleTick(new EventTracker(), i);

        // After completion, further ticks should not advance the entity
        var xAfterCompletion = entity.Location.X;
        ticker.HandleTick(new EventTracker(), Constants.DefaultEffectLength);
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
        var (context, ticker) = MakeContextAndTicker(world, caster: caster);

        var effect = new LinearEngineMotionEffect(
            context: context,
            toMove: new FixedEntitySet(entity),
            origin: new FixedLocation(-1000, 0),
            totalDistanceMm: Constants.DefaultEffectLength * 10,
            tickCount: Constants.DefaultEffectLength,
            isAway: true,
            effectName: "VUN"
        );
        context.EngineAPI.EngineMotion.AddMotionEffect(effect);

        ticker.HandleTick(new EventTracker(), 0); // fails immediately

        // Effect removed — a second tick should produce no further events
        var secondResult = new EventTracker();
        ticker.HandleTick(secondResult, 1);
        secondResult.WorldEvents.Should().BeEmpty();
        entity.Location.X.Should().Be(0);
    }

    [Fact]
    public void TickMotion_ReturnsEvents_FromAllActiveEffects()
    {
        var entity1 = new EntityBuilder().WithLocation(x: 0, y: 0).WithWeight(0).Build();
        var entity2 = new EntityBuilder().WithLocation(x: 0, y: 0).WithWeight(0).Build();
        var world = new WorldModel();
        var (context, ticker) = MakeContextAndTicker(world);

        // Two separate effects — each completes in N ticks, emitting one EntityPushedEvent
        var effect1 = new LinearEngineMotionEffect(
            context: context,
            toMove: new FixedEntitySet(entity1),
            origin: new FixedLocation(-1000, 0),
            totalDistanceMm: Constants.DefaultEffectLength * 10,
            tickCount: Constants.DefaultEffectLength,
            isAway: true,
            effectName: "VUN"
        );
        var effect2 = new LinearEngineMotionEffect(
            context: context,
            toMove: new FixedEntitySet(entity2),
            origin: new FixedLocation(-1000, 0),
            totalDistanceMm: Constants.DefaultEffectLength * 10,
            tickCount: Constants.DefaultEffectLength,
            isAway: true,
            effectName: "VUN"
        );
        context.EngineAPI.EngineMotion.AddMotionEffect(effect1);
        context.EngineAPI.EngineMotion.AddMotionEffect(effect2);

        EventTracker finalResult = new EventTracker();
        for (var i = 0; i < Constants.DefaultEffectLength; i++)
        {
            finalResult = new EventTracker();
            ticker.HandleTick(finalResult, i);
        }

        finalResult.WorldEvents.OfType<EntityPushedEvent>().Should().HaveCount(2);
    }

    [Fact]
    public void TickMotion_ReturnsEmptyResult_WhenNoEffectsActive()
    {
        var world = new WorldModel();
        var ticker = WorldTickerBuilder.ForWorldModel(world).Build();

        var result = new EventTracker();
        ticker.HandleTick(result, 0);

        result.WorldEvents.Should().BeEmpty();
    }

    [Fact]
    public void HandleTick_SetsIsUnderEngineMotion_True_WhenEntityIsUnderActiveEffect()
    {
        var entity = new EntityBuilder().WithLocation(x: 0, y: 0).WithWeight(0).Build();
        var world = new WorldModel();
        world.Add(entity);
        var (context, ticker) = MakeContextAndTicker(world);

        var effect = new LinearEngineMotionEffect(
            context: context,
            toMove: new FixedEntitySet(entity),
            origin: new FixedLocation(-1000, 0),
            totalDistanceMm: Constants.DefaultEffectLength * 10,
            tickCount: Constants.DefaultEffectLength,
            isAway: true,
            effectName: "VUN"
        );
        context.EngineAPI.EngineMotion.AddMotionEffect(effect);

        ticker.HandleTick(new EventTracker(), 0);

        entity.IsUnderEngineMotion.Should().BeTrue();
    }

    [Fact]
    public void HandleTick_SetsIsUnderEngineMotion_False_WhenEntityIsNotUnderAnyEffect()
    {
        var entity = new EntityBuilder().WithLocation(x: 0, y: 0).WithWeight(0).Build();
        var world = new WorldModel();
        world.Add(entity);
        var ticker = WorldTickerBuilder.ForWorldModel(world).Build();

        ticker.HandleTick(new EventTracker(), 0);

        entity.IsUnderEngineMotion.Should().BeFalse();
    }

    [Fact]
    public void HandleTick_ClearsIsUnderEngineMotion_AfterEffectCompletes()
    {
        var entity = new EntityBuilder().WithLocation(x: 0, y: 0).WithWeight(0).Build();
        var world = new WorldModel();
        world.Add(entity);
        var (context, ticker) = MakeContextAndTicker(world);

        var effect = new LinearEngineMotionEffect(
            context: context,
            toMove: new FixedEntitySet(entity),
            origin: new FixedLocation(-1000, 0),
            totalDistanceMm: Constants.DefaultEffectLength * 10,
            tickCount: Constants.DefaultEffectLength,
            isAway: true,
            effectName: "VUN"
        );
        context.EngineAPI.EngineMotion.AddMotionEffect(effect);

        for (var i = 0; i < Constants.DefaultEffectLength; i++) ticker.HandleTick(new EventTracker(), i);

        // Effect is complete and removed; next tick should clear the flag
        ticker.HandleTick(new EventTracker(), Constants.DefaultEffectLength);

        entity.IsUnderEngineMotion.Should().BeFalse();
    }
}
