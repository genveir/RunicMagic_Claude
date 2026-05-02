using RunicMagic.Controller.Services;
using RunicMagic.Tests.Builders;
using RunicMagic.World;
using RunicMagic.World.Capabilities;
using RunicMagic.World.Execution;
using RunicMagic.World.Runes.EffectRunes;

namespace RunicMagic.Tests.Execution.EffectRunes;

public class CJIRTests
{
    // 686 = 2744 / 4, so this is exactly 90° in the 2744-degree circle.
    private const long QuarterTurn = 686;

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
    public void Execute_RotatesEntityClockwise()
    {
        // Entity at (1000, 0). 90° CW around (0, 0) with Y-down puts it at (0, 1000).
        var world = new WorldModel();
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(0).Build();
        var cjir = new CJIR(
            toRotate: new FixedEntitySet(entity),
            howMuch: new FixedNumber(QuarterTurn),
            origin: new FixedLocation(0, 0));
        var context = TestFixtures.MakeContext(world: world);

        cjir.Execute(context);
        RunTicks(world, 56);

        entity.Location.X.Should().BeApproximately(0, 0.001);
        entity.Location.Y.Should().BeApproximately(1000, 0.001);
    }

    [Fact]
    public void Execute_UpdatesEntityAngle()
    {
        var world = new WorldModel();
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(0).Build();
        var expectedAngle = QuarterTurn / 2744.0 * 2 * Math.PI;
        var cjir = new CJIR(
            toRotate: new FixedEntitySet(entity),
            howMuch: new FixedNumber(QuarterTurn),
            origin: new FixedLocation(0, 0));
        var context = TestFixtures.MakeContext(world: world);

        cjir.Execute(context);
        RunTicks(world, 56);

        entity.Angle.Should().BeApproximately(expectedAngle, 0.001);
    }

    [Fact]
    public void Execute_RotationAroundOwnCenter_LocationUnchanged()
    {
        var world = new WorldModel();
        var entity = new EntityBuilder().WithLocation(x: 500, y: 300).Build();
        var cjir = new CJIR(
            toRotate: new FixedEntitySet(entity),
            howMuch: new FixedNumber(QuarterTurn),
            origin: new FixedLocation(500, 300));
        var context = TestFixtures.MakeContext(world: world);

        cjir.Execute(context);
        RunTicks(world, 56);

        entity.Location.X.Should().BeApproximately(500, 0.001);
        entity.Location.Y.Should().BeApproximately(300, 0.001);
    }

    [Fact]
    public void Execute_AddsEntityRotatedEvent()
    {
        var world = new WorldModel();
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(0).Build();
        var cjir = new CJIR(
            toRotate: new FixedEntitySet(entity),
            howMuch: new FixedNumber(QuarterTurn),
            origin: new FixedLocation(0, 0));
        var context = TestFixtures.MakeContext(world: world);

        cjir.Execute(context);
        var finalTickResult = RunTicks(world, 56);

        var rotatedEvent = finalTickResult.WorldEvents.OfType<EntityRotatedEvent>().Should().ContainSingle().Subject;
        rotatedEvent.Entity.Should().BeSameAs(entity);
        rotatedEvent.AngleDegrees.Should().Be(QuarterTurn);
    }

    [Fact]
    public void Execute_ZeroAngle_NoMotionEnqueued()
    {
        var world = new WorldModel();
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).Build();
        var cjir = new CJIR(
            toRotate: new FixedEntitySet(entity),
            howMuch: new FixedNumber(0),
            origin: new FixedLocation(0, 0));
        var context = TestFixtures.MakeContext(world: world);

        cjir.Execute(context);
        var tickResult = new EventTracker();
        world.HandleTick(tickResult);

        tickResult.WorldEvents.OfType<EntityRotatedEvent>().Should().BeEmpty();
        entity.Location.X.Should().Be(1000);
    }

    [Fact]
    public void Execute_PartialPower_StopsAfterAffordableTicks()
    {
        var ticksDrawn = 0;
        var world = new WorldModel();
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).Build();
        entity.Weight = 1_000_000;
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
        var cjir = new CJIR(
            toRotate: new FixedEntitySet(entity),
            howMuch: new FixedNumber(QuarterTurn),
            origin: new FixedLocation(0, 0));
        var context = TestFixtures.MakeContext(caster: caster, world: world);

        cjir.Execute(context);
        for (var i = 0; i < 10; i++) world.HandleTick(new EventTracker());

        // Rotated only 2/56 of the quarter turn; entity should not have reached destination
        entity.Location.X.Should().BeLessThan(1000);
        entity.Location.X.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Execute_EmptySet_NoEvents()
    {
        var world = new WorldModel();
        var cjir = new CJIR(
            toRotate: new FixedEntitySet(),
            howMuch: new FixedNumber(QuarterTurn),
            origin: new FixedLocation(0, 0));
        var context = TestFixtures.MakeContext(world: world);

        cjir.Execute(context);
        var tickResult = new EventTracker();
        world.HandleTick(tickResult);

        tickResult.WorldEvents.Should().BeEmpty();
    }

    [Fact]
    public void Execute_InsufficientPower_DoesNotRotate()
    {
        var world = new WorldModel();
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).Build();
        entity.Weight = 1_000_000;
        var originalX = entity.Location.X;
        var originalY = entity.Location.Y;
        var cjir = new CJIR(
            toRotate: new FixedEntitySet(entity),
            howMuch: new FixedNumber(QuarterTurn),
            origin: new FixedLocation(0, 0));
        var context = TestFixtures.MakeContext(world: world); // no power sources

        cjir.Execute(context);
        var tickResult = new EventTracker();
        world.HandleTick(tickResult);

        entity.Location.X.Should().Be(originalX);
        entity.Location.Y.Should().Be(originalY);
        tickResult.WorldEvents.OfType<EffectNotFiredEvent>().Should().ContainSingle();
        tickResult.WorldEvents.OfType<EntityRotatedEvent>().Should().BeEmpty();
    }

    [Fact]
    public void Execute_DrawsPower_WhenEntityHasWeight()
    {
        var world = new WorldModel();
        var entity = new EntityBuilder()
            .WithLocation(x: 1000, y: 0)
            .WithReservoir(draw: amount => new ReservoirDraw(amount, false))
            .Build();
        entity.Weight = 1_000_000;
        var executor = new EntitySet([entity]);
        var cjir = new CJIR(
            toRotate: new FixedEntitySet(entity),
            howMuch: new FixedNumber(2744), // full circle
            origin: new FixedLocation(0, 0));
        var context = TestFixtures.MakeContext(executor: executor, world: world);

        cjir.Execute(context);
        var allEvents = new List<WorldEvent>();
        EventTracker finalTick = new EventTracker();
        for (var i = 0; i < 56; i++)
        {
            finalTick = new EventTracker();
            world.HandleTick(finalTick);
            allEvents.AddRange(finalTick.WorldEvents);
        }

        allEvents.OfType<PowerDrawnEvent>().Sum(e => e.Amount).Should().BeGreaterThan(0);
        finalTick.WorldEvents.OfType<EntityRotatedEvent>().Should().ContainSingle();
    }
}
