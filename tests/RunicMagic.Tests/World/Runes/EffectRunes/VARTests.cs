using RunicMagic.Controller.Services;
using RunicMagic.World;
using RunicMagic.World.Entities.Capabilities;
using RunicMagic.World.Execution;
using RunicMagic.World.Motion.Engine;
using RunicMagic.World.Runes.EffectRunes;

namespace RunicMagic.Tests.World.Runes.EffectRunes;

public class VARTests
{
    private static EventTracker RunTicks(WorldModel world, int count)
    {
        var ticker = WorldTickerBuilder.ForWorldModel(world).Build();
        var last = new EventTracker();
        for (var i = 0; i < count; i++)
        {
            last = new EventTracker();
            ticker.HandleTick(last, i);
        }
        return last;
    }

    [Fact]
    public void Execute_WithNonZeroDistance_EnqueuesMotion()
    {
        var world = new WorldModelBuilder().Build();
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(0).Build();
        var var_ = new VAR(
            toMove: new FixedEntitySet(entity),
            howFar: new FixedNumber(500),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext(world: world);

        var_.Execute(context);

        var effects = GetPrivateFieldsForTesting.GetEngineMotionCollection(world).GetAll();
        effects.Should().ContainSingle();

        var tracker = new EventTracker();
        var result = effects.Single().TryAdvance(tracker);

        result.EntitiesUnderMotion.Should().ContainSingle().Which.Should().BeSameAs(entity);
    }

    [Fact]
    public void Execute_ZeroDistance_EnqueuesMotion()
    {
        var world = new WorldModelBuilder().Build();
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(0).Build();
        var var_ = new VAR(
            toMove: new FixedEntitySet(entity),
            howFar: new FixedNumber(0),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext(world: world);

        var_.Execute(context);

        var effects = GetPrivateFieldsForTesting.GetEngineMotionCollection(world).GetAll();
        effects.Should().ContainSingle();

        var tracker = new EventTracker();
        var result = effects.Single().TryAdvance(tracker);

        result.EntitiesUnderMotion.Should().ContainSingle().Which.Should().BeSameAs(entity);
    }

    [Fact]
    public void Execute_PullsEntityTowardsOrigin()
    {
        var world = new WorldModelBuilder().Build();
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(0).Build();
        var var_ = new VAR(
            toMove: new FixedEntitySet(entity),
            howFar: new FixedNumber(500),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext(world: world);

        var_.Execute(context);
        RunTicks(world, Constants.DefaultEffectLength);

        entity.Location.X.Should().BeApproximately(500, 0.001);
        entity.Location.Y.Should().BeApproximately(0, 0.001);
    }

    [Fact]
    public void Execute_NullVector_EntityStillMoves()
    {
        var world = new WorldModelBuilder().Build();
        var entity = new EntityBuilder().WithLocation(x: 0, y: 0).WithWeight(0).Build();
        var var_ = new VAR(
            toMove: new FixedEntitySet(entity),
            howFar: new FixedNumber(100),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext(world: world);

        var_.Execute(context);
        RunTicks(world, Constants.DefaultEffectLength);

        var displaced = entity.Location.X != 0 || entity.Location.Y != 0;
        displaced.Should().BeTrue();
    }

    [Fact]
    public void Execute_DrawsPowerFromCaster()
    {
        var drawn = new List<long>();
        var casterEntity = new EntityBuilder()
            .WithReservoir(draw: amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); })
            .Build();
        var caster = new EntitySet([casterEntity]);

        // N mm × 1g = N total cost; 1 per tick
        var world = new WorldModelBuilder().Build();
        var target = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(1).Build();
        var var_ = new VAR(
            toMove: new FixedEntitySet(target),
            howFar: new FixedNumber(Constants.DefaultEffectLength),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext(caster: caster, world: world);

        var_.Execute(context);
        RunTicks(world, Constants.DefaultEffectLength);

        drawn.Should().HaveCount(Constants.DefaultEffectLength);
        drawn.Sum().Should().Be(Constants.DefaultEffectLength);
    }

    [Fact]
    public void Execute_DrawsFromExecutorFirst_ThenCaster()
    {
        var executorDrawn = new List<long>();
        var casterDrawn = new List<long>();

        var executorEntity = new EntityBuilder()
            .WithReservoir(draw: amount => { executorDrawn.Add(amount); return new ReservoirDraw(amount / 2, false); })
            .Build();
        var executor = new EntitySet([executorEntity]);

        var casterEntity = new EntityBuilder()
            .WithReservoir(draw: amount => { casterDrawn.Add(amount); return new ReservoirDraw(amount, false); })
            .Build();
        var caster = new EntitySet([casterEntity]);

        // 2N mm × 1g = 2N total cost; 2 per tick
        // executor provides 1 per tick (amount/2), caster covers remaining 1
        var world = new WorldModelBuilder().Build();
        var target = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(1).Build();
        var var_ = new VAR(
            toMove: new FixedEntitySet(target),
            howFar: new FixedNumber(Constants.DefaultEffectLength * 2),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext(caster: caster, executor: executor, world: world);

        var_.Execute(context);
        RunTicks(world, Constants.DefaultEffectLength);

        executorDrawn.Should().HaveCount(Constants.DefaultEffectLength);
        executorDrawn.All(x => x == 2).Should().BeTrue();
        casterDrawn.Should().HaveCount(Constants.DefaultEffectLength);
        casterDrawn.All(x => x == 1).Should().BeTrue();
    }

    [Fact]
    public void Execute_MultipleEntities_AllMoveTowardsOrigin()
    {
        var world = new WorldModelBuilder().Build();
        var entity1 = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(0).Build();
        var entity2 = new EntityBuilder().WithLocation(x: 0, y: 1000).WithWeight(0).Build();
        var var_ = new VAR(
            toMove: new FixedEntitySet(entity1, entity2),
            howFar: new FixedNumber(200),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext(world: world);

        var_.Execute(context);
        RunTicks(world, Constants.DefaultEffectLength);

        entity1.Location.X.Should().BeApproximately(800, 0.001);
        entity1.Location.Y.Should().BeApproximately(0, 0.001);
        entity2.Location.X.Should().BeApproximately(0, 0.001);
        entity2.Location.Y.Should().BeApproximately(800, 0.001);
    }

    [Fact]
    public void Execute_EmitsEntityPulledEvent_PerEntity_OnFinalTick()
    {
        var world = new WorldModelBuilder().Build();
        var entity1 = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(0).Build();
        var entity2 = new EntityBuilder().WithLocation(x: 0, y: 1000).WithWeight(0).Build();
        var var_ = new VAR(
            toMove: new FixedEntitySet(entity1, entity2),
            howFar: new FixedNumber(200),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext(world: world);

        var_.Execute(context);
        var finalTickResult = RunTicks(world, Constants.DefaultEffectLength);

        finalTickResult.WorldEvents.OfType<EntityPulledEvent>().Should().HaveCount(2);
    }

    [Fact]
    public void Execute_InsufficientPower_DoesNotMoveAndEmitsEvent()
    {
        // 1000mm × 1_000_000g = 1,000,000,000 total cost
        var world = new WorldModelBuilder().Build();
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(1_000_000).Build();
        var originalX = entity.Location.X;

        var casterEntity = new EntityBuilder()
            .WithReservoir(draw: amount => new ReservoirDraw(0, false))
            .Build();
        var caster = new EntitySet([casterEntity]);

        var var_ = new VAR(
            toMove: new FixedEntitySet(entity),
            howFar: new FixedNumber(1000),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext(caster: caster, world: world);

        var_.Execute(context);
        var ticker = WorldTickerBuilder.ForWorldModel(world).Build();
        var tickResult = new EventTracker();
        ticker.HandleTick(tickResult, 0);

        entity.Location.X.Should().Be(originalX);
        tickResult.WorldEvents.OfType<EffectNotFiredEvent>().Should().ContainSingle()
            .Which.Effect.Should().Be("VAR");
    }

    [Fact]
    public void Execute_PartialPower_StopsAfterAffordableTicks()
    {
        // Give the caster power for exactly 2 ticks
        var ticksDrawn = 0;
        var world = new WorldModelBuilder().Build();
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(Constants.DefaultEffectLength * 1000).Build();
        var casterEntity = new EntityBuilder()
            .WithReservoir(draw: amount =>
            {
                if (ticksDrawn < 2)
                {
                    ticksDrawn++;
                    return new ReservoirDraw(amount, false);
                }
                return new ReservoirDraw(0, false);
            })
            .Build();
        var caster = new EntitySet([casterEntity]);

        var var_ = new VAR(
            toMove: new FixedEntitySet(entity),
            howFar: new FixedNumber(1000),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext(caster: caster, world: world);

        var_.Execute(context);
        var ticker = WorldTickerBuilder.ForWorldModel(world).Build();
        for (var i = 0; i < 10; i++) ticker.HandleTick(new EventTracker(), i);

        // Moved only 2 ticks worth; entity should not be at full destination
        entity.Location.X.Should().BeLessThan(1000);
        entity.Location.X.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Execute_EmptySet_DrawsNoPower()
    {
        var drawn = new List<long>();
        var casterEntity = new EntityBuilder()
            .WithReservoir(draw: amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); })
            .Build();
        var caster = new EntitySet([casterEntity]);

        var world = new WorldModelBuilder().Build();
        var var_ = new VAR(
            toMove: new FixedEntitySet(),
            howFar: new FixedNumber(500),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext(caster: caster, world: world);

        var_.Execute(context);
        var ticker = WorldTickerBuilder.ForWorldModel(world).Build();
        ticker.HandleTick(new EventTracker(), 0);

        drawn.Should().BeEmpty();
    }
}
