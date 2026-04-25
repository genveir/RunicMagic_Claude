using FluentAssertions;
using RunicMagic.Tests.Builders;
using RunicMagic.World.Capabilities;
using RunicMagic.World.Execution;
using RunicMagic.World.Runes.EffectRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.EffectRunes;

public class CJARTests
{
    // 686 = 2744 / 4, so this is exactly 90° in the 2744-degree circle.
    private const long QuarterTurn = 686;

    [Fact]
    public void Execute_RotatesEntityCounterclockwise()
    {
        // Entity at (1000, 0). 90° CCW around (0, 0) with Y-down puts it at (0, -1000).
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).Build();
        var cjar = new CJAR(
            toRotate: new FixedEntitySet(entity),
            howMuch: new FixedNumber(QuarterTurn),
            origin: new FixedLocation(0, 0));
        var context = TestFixtures.MakeContext();

        cjar.Execute(context);

        entity.Location.X.Should().BeApproximately(0, 0.001);
        entity.Location.Y.Should().BeApproximately(-1000, 0.001);
    }

    [Fact]
    public void Execute_UpdatesEntityAngle()
    {
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).Build();
        var expectedAngle = -(QuarterTurn / 2744.0 * 2 * Math.PI);
        var cjar = new CJAR(
            toRotate: new FixedEntitySet(entity),
            howMuch: new FixedNumber(QuarterTurn),
            origin: new FixedLocation(0, 0));
        var context = TestFixtures.MakeContext();

        cjar.Execute(context);

        entity.Angle.Should().BeApproximately(expectedAngle, 0.001);
    }

    [Fact]
    public void Execute_RotationAroundOwnCenter_LocationUnchanged()
    {
        var entity = new EntityBuilder().WithLocation(x: 500, y: 300).Build();
        var cjar = new CJAR(
            toRotate: new FixedEntitySet(entity),
            howMuch: new FixedNumber(QuarterTurn),
            origin: new FixedLocation(500, 300));
        var context = TestFixtures.MakeContext();

        cjar.Execute(context);

        entity.Location.X.Should().BeApproximately(500, 0.001);
        entity.Location.Y.Should().BeApproximately(300, 0.001);
    }

    [Fact]
    public void Execute_AddsEntityRotatedEvent()
    {
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).Build();
        var cjar = new CJAR(
            toRotate: new FixedEntitySet(entity),
            howMuch: new FixedNumber(QuarterTurn),
            origin: new FixedLocation(0, 0));
        var context = TestFixtures.MakeContext();

        cjar.Execute(context);

        var rotatedEvent = context.Result.Events.OfType<EntityRotatedEvent>().Should().ContainSingle().Subject;
        rotatedEvent.Entity.Should().BeSameAs(entity);
        rotatedEvent.AngleDegrees.Should().Be(QuarterTurn);
    }

    [Fact]
    public void Execute_ZeroAngle_NoRotatedEvent()
    {
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).Build();
        var cjar = new CJAR(
            toRotate: new FixedEntitySet(entity),
            howMuch: new FixedNumber(0),
            origin: new FixedLocation(0, 0));
        var context = TestFixtures.MakeContext();

        cjar.Execute(context);

        context.Result.Events.OfType<EntityRotatedEvent>().Should().BeEmpty();
    }

    [Fact]
    public void Execute_EmptySet_NoEvents()
    {
        var cjar = new CJAR(
            toRotate: new FixedEntitySet(),
            howMuch: new FixedNumber(QuarterTurn),
            origin: new FixedLocation(0, 0));
        var context = TestFixtures.MakeContext();

        cjar.Execute(context);

        context.Result.Events.Should().BeEmpty();
    }

    [Fact]
    public void Execute_InsufficientPower_DoesNotRotate()
    {
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).Build();
        entity.Weight = 1_000_000;
        var originalX = entity.Location.X;
        var originalY = entity.Location.Y;
        var cjar = new CJAR(
            toRotate: new FixedEntitySet(entity),
            howMuch: new FixedNumber(QuarterTurn),
            origin: new FixedLocation(0, 0));
        var context = TestFixtures.MakeContext(); // no power sources

        cjar.Execute(context);

        entity.Location.X.Should().Be(originalX);
        entity.Location.Y.Should().Be(originalY);
        context.Result.Events.OfType<EffectNotFiredEvent>().Should().ContainSingle();
        context.Result.Events.OfType<EntityRotatedEvent>().Should().BeEmpty();
    }

    [Fact]
    public void Execute_DrawsPower_WhenEntityHasWeight()
    {
        var entity = new EntityBuilder()
            .WithLocation(x: 0, y: 0)
            .WithReservoir(draw: amount => new ReservoirDraw(amount, false))
            .Build();
        entity.Weight = 1_000_000;
        var executor = new EntitySet([entity]);
        var cjar = new CJAR(
            toRotate: new FixedEntitySet(entity),
            howMuch: new FixedNumber(2744), // full circle
            origin: new FixedLocation(0, 0));
        var context = TestFixtures.MakeContext(executor: executor);

        cjar.Execute(context);

        context.Result.Events.OfType<PowerDrawnEvent>().Sum(e => e.Amount).Should().BeGreaterThan(0);
        context.Result.Events.OfType<EntityRotatedEvent>().Should().ContainSingle();
    }

    [Fact]
    public void Execute_CostMatchesCJIR_ForSameInputs()
    {
        // Both runes should draw the same amount of power for the same entity and angle.
        long drawnByCJIR = 0;
        var entityA = new EntityBuilder()
            .WithLocation(x: 0, y: 0)
            .WithReservoir(draw: amount => { drawnByCJIR = amount; return new ReservoirDraw(amount, false); })
            .Build();
        entityA.Weight = 1_000_000;

        long drawnByCJAR = 0;
        var entityB = new EntityBuilder()
            .WithLocation(x: 0, y: 0)
            .WithReservoir(draw: amount => { drawnByCJAR = amount; return new ReservoirDraw(amount, false); })
            .Build();
        entityB.Weight = 1_000_000;

        var cjir = new CJIR(
            toRotate: new FixedEntitySet(entityA),
            howMuch: new FixedNumber(2744),
            origin: new FixedLocation(0, 0));
        var cjar = new CJAR(
            toRotate: new FixedEntitySet(entityB),
            howMuch: new FixedNumber(2744),
            origin: new FixedLocation(0, 0));

        cjir.Execute(TestFixtures.MakeContext(executor: new EntitySet([entityA])));
        cjar.Execute(TestFixtures.MakeContext(executor: new EntitySet([entityB])));

        drawnByCJIR.Should().Be(drawnByCJAR);
    }
}
