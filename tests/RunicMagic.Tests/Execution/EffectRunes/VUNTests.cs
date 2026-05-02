using RunicMagic.Controller.Services;
using RunicMagic.World;
using RunicMagic.World.Capabilities;
using RunicMagic.World.Execution;
using RunicMagic.World.Runes.EffectRunes;

namespace RunicMagic.Tests.Execution.EffectRunes;

public class VUNTests
{
    private static EventTracker RunTicks(WorldModel world, int count)
    {
        var last = new EventTracker();
        for (var i = 0; i < count; i++)
        {
            last = new EventTracker();
            world.HandleTick(last);
        }
        return last;
    }

    [Fact]
    public void Execute_PushesEntityAwayFromOrigin()
    {
        var world = new WorldModel();
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(0).Build();
        var vun = new VUN(
            toMove: new FixedEntitySet(entity),
            howFar: new FixedNumber(500),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext(world: world);

        vun.Execute(context);
        RunTicks(world, 56);

        entity.Location.X.Should().BeApproximately(1500, 0.001);
        entity.Location.Y.Should().BeApproximately(0, 0.001);
    }

    [Fact]
    public void Execute_NullVector_EntityStillMoves()
    {
        var world = new WorldModel();
        var entity = new EntityBuilder().WithLocation(x: 0, y: 0).WithWeight(0).Build();
        var vun = new VUN(
            toMove: new FixedEntitySet(entity),
            howFar: new FixedNumber(100),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext(world: world);

        vun.Execute(context);
        RunTicks(world, 56);

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

        // 56mm × 1g = 56 total cost; 56 / 56 = 1 per tick
        var world = new WorldModel();
        var target = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(1).Build();
        var vun = new VUN(
            toMove: new FixedEntitySet(target),
            howFar: new FixedNumber(56),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext(caster: caster, world: world);

        vun.Execute(context);
        RunTicks(world, 56);

        drawn.Should().HaveCount(56);
        drawn.Sum().Should().Be(56);
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

        // 112mm × 1g = 112 total cost; 112 / 56 = 2 per tick
        // executor provides 1 per tick (amount/2), caster covers remaining 1
        var world = new WorldModel();
        var target = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(1).Build();
        var vun = new VUN(
            toMove: new FixedEntitySet(target),
            howFar: new FixedNumber(112),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext(caster: caster, executor: executor, world: world);

        vun.Execute(context);
        RunTicks(world, 56);

        executorDrawn.Should().HaveCount(56);
        executorDrawn.All(x => x == 2).Should().BeTrue();
        casterDrawn.Should().HaveCount(56);
        casterDrawn.All(x => x == 1).Should().BeTrue();
    }

    [Fact]
    public void Execute_MultipleEntities_AllMoveAwayFromOrigin()
    {
        var world = new WorldModel();
        var entity1 = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(0).Build();
        var entity2 = new EntityBuilder().WithLocation(x: 0, y: 1000).WithWeight(0).Build();
        var vun = new VUN(
            toMove: new FixedEntitySet(entity1, entity2),
            howFar: new FixedNumber(200),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext(world: world);

        vun.Execute(context);
        RunTicks(world, 56);

        entity1.Location.X.Should().BeApproximately(1200, 0.001);
        entity1.Location.Y.Should().BeApproximately(0, 0.001);
        entity2.Location.X.Should().BeApproximately(0, 0.001);
        entity2.Location.Y.Should().BeApproximately(1200, 0.001);
    }

    [Fact]
    public void Execute_EmitsEntityPushedEvent_PerEntity()
    {
        var world = new WorldModel();
        var entity1 = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(0).Build();
        var entity2 = new EntityBuilder().WithLocation(x: 0, y: 1000).WithWeight(0).Build();
        var vun = new VUN(
            toMove: new FixedEntitySet(entity1, entity2),
            howFar: new FixedNumber(200),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext(world: world);

        vun.Execute(context);
        var finalTickResult = RunTicks(world, 56);

        finalTickResult.WorldEvents.OfType<EntityPushedEvent>().Should().HaveCount(2);
    }

    [Fact]
    public void Execute_InsufficientPower_DoesNotMoveAndEmitsEvent()
    {
        // 1000mm × 1_000_000g = 1,000,000,000 total cost
        var world = new WorldModel();
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(1_000_000).Build();
        var originalX = entity.Location.X;

        var casterEntity = new EntityBuilder()
            .WithReservoir(draw: amount => new ReservoirDraw(0, false))
            .Build();
        var caster = new EntitySet([casterEntity]);

        var vun = new VUN(
            toMove: new FixedEntitySet(entity),
            howFar: new FixedNumber(1000),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext(caster: caster, world: world);

        vun.Execute(context);
        var tickResult = new EventTracker();
        world.HandleTick(tickResult);

        entity.Location.X.Should().Be(originalX);
        tickResult.WorldEvents.OfType<EffectNotFiredEvent>().Should().ContainSingle()
            .Which.Effect.Should().Be("VUN");
    }

    [Fact]
    public void Execute_PartialPower_StopsAfterAffordableTicks()
    {
        // 1000mm × 56000g = 56,000,000 total cost; 1,000,000 per tick
        // Give the caster power for exactly 2 ticks
        var ticksDrawn = 0;
        var world = new WorldModel();
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(56000).Build();
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

        var vun = new VUN(
            toMove: new FixedEntitySet(entity),
            howFar: new FixedNumber(1000),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext(caster: caster, world: world);

        vun.Execute(context);
        for (var i = 0; i < 10; i++) world.HandleTick(new EventTracker());

        // Moved 2/56 of the total distance; entity should not be at full destination
        entity.Location.X.Should().BeGreaterThan(1000);
        entity.Location.X.Should().BeLessThan(2000);
    }

    [Fact]
    public void Execute_EmptySet_DrawsNoPower()
    {
        var drawn = new List<long>();
        var casterEntity = new EntityBuilder()
            .WithReservoir(draw: amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); })
            .Build();
        var caster = new EntitySet([casterEntity]);

        var world = new WorldModel();
        var vun = new VUN(
            toMove: new FixedEntitySet(),
            howFar: new FixedNumber(500),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext(caster: caster, world: world);

        vun.Execute(context);
        world.HandleTick(new EventTracker());

        drawn.Should().BeEmpty();
    }
}
