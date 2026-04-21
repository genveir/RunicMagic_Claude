using FluentAssertions;
using RunicMagic.World;
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
        var entity = TestFixtures.MakeEntity(x: 1000, y: 0);
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
        var entity = TestFixtures.MakeEntity(x: 1000, y: 0);
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
        var entity = TestFixtures.MakeEntity(x: 500, y: 300);
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
        var entity = TestFixtures.MakeEntity(x: 1000, y: 0);
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
        var entity = TestFixtures.MakeEntity(x: 1000, y: 0);
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
        var entity = TestFixtures.MakeEntity(x: 1000, y: 0);
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
        var entity = TestFixtures.MakeEntity(x: 0, y: 0);
        entity.Weight = 1_000_000;
        entity.Reservoir = amount => new ReservoirDraw(amount, false);
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
        var entityA = TestFixtures.MakeEntity(x: 0, y: 0);
        entityA.Weight = 1_000_000;
        long drawnByCJIR = 0;
        entityA.Reservoir = amount => { drawnByCJIR = amount; return new ReservoirDraw(amount, false); };

        var entityB = TestFixtures.MakeEntity(x: 0, y: 0);
        entityB.Weight = 1_000_000;
        long drawnByCJAR = 0;
        entityB.Reservoir = amount => { drawnByCJAR = amount; return new ReservoirDraw(amount, false); };

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
