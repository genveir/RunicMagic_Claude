using RunicMagic.Controller.Services;
using RunicMagic.World;
using RunicMagic.World.Entities.Capabilities;
using RunicMagic.World.Execution;
using RunicMagic.World.Motion.Engine;
using RunicMagic.World.Runes.EffectRunes;

namespace RunicMagic.Tests.World.Runes.EffectRunes;

public class CJARTests
{
    // 686 = 2744 / 4, so this is exactly 90° in the 2744-degree circle.
    private const long QuarterTurn = 686;

    private static void RunTicks(SpellHarness harness, int count, IWorldEventTracker worldEventTracker)
    {
        var ticker = harness.BuildTicker();
        for (var i = 0; i < count; i++)
        {
            ticker.HandleTick(worldEventTracker, i);
        }
    }

    [Fact]
    public void Execute_RotatesEntityCounterclockwise()
    {
        // Entity at (1000, 0). 90° CCW around (0, 0) with Y-down puts it at (0, -1000).
        var harness = new SpellHarness();
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(0).Build();
        var cjar = new CJAR(
            toRotate: new FixedEntitySet(entity),
            howMuch: new FixedNumber(QuarterTurn),
            origin: new FixedLocation(0, 0));
        var context = harness.MakeContext();

        cjar.Execute(context);
        RunTicks(harness, Constants.DefaultEffectLength, context.EventTracker);

        entity.Location.X.Should().BeApproximately(0, 0.001);
        entity.Location.Y.Should().BeApproximately(-1000, 0.001);
    }

    [Fact]
    public void Execute_UpdatesEntityAngle()
    {
        var harness = new SpellHarness();
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(0).Build();
        var expectedAngle = -(QuarterTurn / 2744.0 * 2 * Math.PI);
        var cjar = new CJAR(
            toRotate: new FixedEntitySet(entity),
            howMuch: new FixedNumber(QuarterTurn),
            origin: new FixedLocation(0, 0));
        var context = harness.MakeContext();

        cjar.Execute(context);
        RunTicks(harness, Constants.DefaultEffectLength, new EventTracker());

        entity.FacingAngle.Should().BeApproximately(expectedAngle, 0.001);
    }

    [Fact]
    public void Execute_RotationAroundOwnCenter_LocationUnchanged()
    {
        var harness = new SpellHarness();
        var entity = new EntityBuilder().WithLocation(x: 500, y: 300).Build();
        var cjar = new CJAR(
            toRotate: new FixedEntitySet(entity),
            howMuch: new FixedNumber(QuarterTurn),
            origin: new FixedLocation(500, 300));
        var context = harness.MakeContext();

        cjar.Execute(context);
        RunTicks(harness, Constants.DefaultEffectLength, new EventTracker());

        entity.Location.X.Should().BeApproximately(500, 0.001);
        entity.Location.Y.Should().BeApproximately(300, 0.001);
    }

    [Fact]
    public void Execute_WithNonZeroAngle_EnqueuesMotion()
    {
        var harness = new SpellHarness();
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(0).Build();
        var cjar = new CJAR(
            toRotate: new FixedEntitySet(entity),
            howMuch: new FixedNumber(QuarterTurn),
            origin: new FixedLocation(0, 0));
        var context = harness.MakeContext();

        cjar.Execute(context);

        var effects = harness.EngineMotion.GetAll();
        effects.Should().ContainSingle();

        var tracker = new EventTracker();
        var result = effects.Single().TryAdvance(tracker);

        result.EntitiesUnderMotion.Should().ContainSingle().Which.Should().BeSameAs(entity);
    }

    [Fact]
    public void Execute_AddsEntityRotatedEvent_OnFinalTick()
    {
        var harness = new SpellHarness();
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(0).Build();
        var cjar = new CJAR(
            toRotate: new FixedEntitySet(entity),
            howMuch: new FixedNumber(QuarterTurn),
            origin: new FixedLocation(0, 0));
        var context = harness.MakeContext();

        cjar.Execute(context);
        var ticker = harness.BuildTicker();
        for (var i = 0; i < Constants.DefaultEffectLength - 1; i++) ticker.HandleTick(new EventTracker(), i);
        var finalTickResult = new EventTracker();
        ticker.HandleTick(finalTickResult, Constants.DefaultEffectLength - 1);

        var rotatedEvent = finalTickResult.WorldEvents.OfType<EntityRotatedEvent>().Should().ContainSingle().Subject;
        rotatedEvent.Entity.Should().BeSameAs(entity);
        rotatedEvent.AngleDegrees.Should().Be(QuarterTurn);
    }

    [Fact]
    public void Execute_ZeroAngle_EnqueuesMotion()
    {
        var harness = new SpellHarness();
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(0).Build();
        var cjar = new CJAR(
            toRotate: new FixedEntitySet(entity),
            howMuch: new FixedNumber(0),
            origin: new FixedLocation(0, 0));
        var context = harness.MakeContext();

        cjar.Execute(context);

        var effects = harness.EngineMotion.GetAll();
        effects.Should().ContainSingle();

        var tracker = new EventTracker();
        var result = effects.Single().TryAdvance(tracker);

        result.EntitiesUnderMotion.Should().ContainSingle().Which.Should().BeSameAs(entity);
    }

    [Fact]
    public void Execute_PartialPower_StopsAfterAffordableTicks()
    {
        var ticksDrawn = 0;
        var harness = new SpellHarness();
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
        var cjar = new CJAR(
            toRotate: new FixedEntitySet(entity),
            howMuch: new FixedNumber(QuarterTurn),
            origin: new FixedLocation(0, 0));
        var context = harness.MakeContext(caster: caster);

        cjar.Execute(context);
        var ticker = harness.BuildTicker();
        for (var i = 0; i < 10; i++) ticker.HandleTick(new EventTracker(), i);

        // Rotated only 2 ticks worth; entity should not have reached destination
        entity.Location.X.Should().BeLessThan(1000);
        entity.Location.X.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Execute_EmptySet_NoEvents()
    {
        var harness = new SpellHarness();
        var cjar = new CJAR(
            toRotate: new FixedEntitySet(),
            howMuch: new FixedNumber(QuarterTurn),
            origin: new FixedLocation(0, 0));
        var context = harness.MakeContext();

        cjar.Execute(context);
        var ticker = harness.BuildTicker();
        var tickResult = new EventTracker();
        ticker.HandleTick(tickResult, 0);

        tickResult.WorldEvents.Should().BeEmpty();
    }

    [Fact]
    public void Execute_InsufficientPower_DoesNotRotate()
    {
        var harness = new SpellHarness();
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).Build();
        entity.Weight = 1_000_000;
        var originalX = entity.Location.X;
        var originalY = entity.Location.Y;
        var cjar = new CJAR(
            toRotate: new FixedEntitySet(entity),
            howMuch: new FixedNumber(QuarterTurn),
            origin: new FixedLocation(0, 0));
        var context = harness.MakeContext(); // no power sources

        cjar.Execute(context);
        var ticker = harness.BuildTicker();
        var tickResult = new EventTracker();
        ticker.HandleTick(tickResult, 0);

        entity.Location.X.Should().Be(originalX);
        entity.Location.Y.Should().Be(originalY);
        tickResult.WorldEvents.OfType<EffectNotFiredEvent>().Should().ContainSingle();
        tickResult.WorldEvents.OfType<EntityRotatedEvent>().Should().BeEmpty();
    }

    [Fact]
    public void Execute_DrawsPower_WhenEntityHasWeight()
    {
        var harness = new SpellHarness();
        var entity = new EntityBuilder()
            .WithLocation(x: 1000, y: 0)
            .WithReservoir(draw: amount => new ReservoirDraw(amount, false))
            .Build();
        entity.Weight = 1_000_000;
        var executor = new EntitySet([entity]);
        var cjar = new CJAR(
            toRotate: new FixedEntitySet(entity),
            howMuch: new FixedNumber(2744), // full circle
            origin: new FixedLocation(0, 0));
        var context = harness.MakeContext(executor: executor);

        cjar.Execute(context);
        var ticker = harness.BuildTicker();
        var allEvents = new List<WorldEvent>();
        EventTracker finalTick = new EventTracker();
        for (var i = 0; i < Constants.DefaultEffectLength; i++)
        {
            finalTick = new EventTracker();
            ticker.HandleTick(finalTick, i);
            allEvents.AddRange(finalTick.WorldEvents);
        }

        allEvents.OfType<PowerDrawnEvent>().Sum(e => e.Amount).Should().BeGreaterThan(0);
        finalTick.WorldEvents.OfType<EntityRotatedEvent>().Should().ContainSingle();
    }

    [Fact]
    public void Execute_CostMatchesCJIR_ForSameInputs()
    {
        // Both runes should draw the same total power for the same entity and angle.
        var totalDrawnByCJIR = 0L;
        var entityA = new EntityBuilder()
            .WithLocation(x: 0, y: 0)
            .WithReservoir(draw: amount => { totalDrawnByCJIR += amount; return new ReservoirDraw(amount, false); })
            .Build();
        entityA.Weight = 1_000_000;

        var totalDrawnByCJAR = 0L;
        var entityB = new EntityBuilder()
            .WithLocation(x: 0, y: 0)
            .WithReservoir(draw: amount => { totalDrawnByCJAR += amount; return new ReservoirDraw(amount, false); })
            .Build();
        entityB.Weight = 1_000_000;

        var harnessA = new SpellHarness();
        var harnessB = new SpellHarness();

        var cjir = new CJIR(
            toRotate: new FixedEntitySet(entityA),
            howMuch: new FixedNumber(2744),
            origin: new FixedLocation(0, 0));
        var cjar = new CJAR(
            toRotate: new FixedEntitySet(entityB),
            howMuch: new FixedNumber(2744),
            origin: new FixedLocation(0, 0));

        cjir.Execute(harnessA.MakeContext(executor: new EntitySet([entityA])));
        cjar.Execute(harnessB.MakeContext(executor: new EntitySet([entityB])));

        var tickerA = harnessA.BuildTicker();
        var tickerB = harnessB.BuildTicker();
        for (var i = 0; i < Constants.DefaultEffectLength; i++) tickerA.HandleTick(new EventTracker(), i);
        for (var i = 0; i < Constants.DefaultEffectLength; i++) tickerB.HandleTick(new EventTracker(), i);

        totalDrawnByCJIR.Should().Be(totalDrawnByCJAR);
    }
}
