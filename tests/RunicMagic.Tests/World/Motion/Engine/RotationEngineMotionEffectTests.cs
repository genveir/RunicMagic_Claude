using RunicMagic.Controller.Services;
using RunicMagic.World.Entities;
using RunicMagic.World.Entities.Capabilities;
using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Motion.Engine;

namespace RunicMagic.Tests.World.Motion.Engine;

public class RotationEngineMotionEffectTests
{
    private const long QuarterTurn = 686; // 2744 / 4

    private static RotationEngineMotionEffect MakeCwEffect(
        SpellContext context,
        Entity entity,
        long totalRuneDegrees)
    {
        var totalTheta = totalRuneDegrees / 2744.0 * 2 * Math.PI;
        return new RotationEngineMotionEffect(
            context: context,
            toMove: new FixedEntitySet(entity),
            origin: new FixedLocation(0, 0),
            perTickTheta: totalTheta / 56.0,
            totalRuneDegrees: totalRuneDegrees,
            effectName: "CJIR"
        );
    }

    [Fact]
    public void TryAdvance_After56Ticks_EntityCompletesFullRotation()
    {
        // Entity at (1000, 0). Full 90° CW rotation puts it at (0, 1000).
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(0).Build();
        var context = TestFixtures.MakeContext();
        var effect = MakeCwEffect(context, entity, totalRuneDegrees: QuarterTurn);

        for (var i = 0; i < 56; i++) effect.TryAdvance(new EventTracker());

        entity.Location.X.Should().BeApproximately(0, 0.001);
        entity.Location.Y.Should().BeApproximately(1000, 0.001);
        effect.IsComplete.Should().BeTrue();
    }

    [Fact]
    public void TryAdvance_EmitsEntityRotatedEvent_OnLastTick()
    {
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(0).Build();
        var context = TestFixtures.MakeContext();
        var effect = MakeCwEffect(context, entity, totalRuneDegrees: QuarterTurn);

        EventTracker lastResult = new EventTracker();
        for (var i = 0; i < 56; i++)
        {
            lastResult = new EventTracker();
            effect.TryAdvance(lastResult);
        }

        lastResult.WorldEvents.OfType<EntityRotatedEvent>().Should().ContainSingle()
            .Which.AngleDegrees.Should().Be(QuarterTurn);
    }

    [Fact]
    public void TryAdvance_NoEntityRotatedEvent_OnIntermediateTicks()
    {
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).Build();
        var context = TestFixtures.MakeContext();
        var effect = MakeCwEffect(context, entity, totalRuneDegrees: QuarterTurn);

        var intermediateResult = new EventTracker();
        effect.TryAdvance(intermediateResult);

        intermediateResult.WorldEvents.OfType<EntityRotatedEvent>().Should().BeEmpty();
    }

    [Fact]
    public void TryAdvance_InsufficientPower_ReturnsFalseAndEmitsEvent()
    {
        // Entity with high weight at distance from origin so cost is non-zero
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).Build();
        entity.Weight = 1_000_000;

        var casterEntity = new EntityBuilder()
            .WithReservoir(draw: amount => new ReservoirDraw(0, false))
            .Build();
        var caster = new EntitySet([casterEntity]);
        var context = TestFixtures.MakeContext(caster: caster);

        var effect = MakeCwEffect(context, entity, totalRuneDegrees: QuarterTurn);

        var result = new EventTracker();
        var (advanced, _) = effect.TryAdvance(result);

        advanced.Should().BeFalse();
        result.WorldEvents.OfType<EffectNotFiredEvent>().Should().ContainSingle()
            .Which.Effect.Should().Be("CJIR");
    }

    [Fact]
    public void TryAdvance_InsufficientPower_EntityDoesNotRotate()
    {
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).Build();
        entity.Weight = 1_000_000;
        var originalX = entity.Location.X;

        var casterEntity = new EntityBuilder()
            .WithReservoir(draw: amount => new ReservoirDraw(0, false))
            .Build();
        var caster = new EntitySet([casterEntity]);
        var context = TestFixtures.MakeContext(caster: caster);

        var effect = MakeCwEffect(context, entity, totalRuneDegrees: QuarterTurn);
        effect.TryAdvance(new EventTracker());

        entity.Location.X.Should().Be(originalX);
    }

    [Fact]
    public void TryAdvance_DynamicCost_IncreasesWhenRadiusGrows()
    {
        // Simulate a VUN concurrently pushing the entity outward during rotation.
        // We manually move the entity before each tick to simulate increasing radius.
        var entity = new EntityBuilder()
            .WithLocation(x: 1000, y: 0)
            .WithReservoir(draw: amount => new ReservoirDraw(amount, false))
            .Build();
        entity.Weight = 1_000_000;

        var powerSource = new EntitySet([entity]);
        var context = TestFixtures.MakeContext(caster: powerSource);

        var totalTheta = QuarterTurn / 2744.0 * 2 * Math.PI;
        var effect = new RotationEngineMotionEffect(
            context: context,
            toMove: new FixedEntitySet(entity),
            origin: new FixedLocation(0, 0),
            perTickTheta: totalTheta / 56.0,
            totalRuneDegrees: QuarterTurn,
            effectName: "CJIR"
        );

        // First tick at radius 1000
        var result1 = new EventTracker();
        effect.TryAdvance(result1);
        var costAtRadius1000 = result1.WorldEvents.OfType<PowerDrawnEvent>().Sum(e => e.Amount);

        // Manually move the entity to a larger radius before the second tick.
        // Reset angle to 0 as well so the cost comparison is purely due to radius.
        entity.Location = new Location(2000, 0);
        entity.FacingAngle = 0;

        var result2 = new EventTracker();
        effect.TryAdvance(result2);
        var costAtRadius2000 = result2.WorldEvents.OfType<PowerDrawnEvent>().Sum(e => e.Amount);

        costAtRadius2000.Should().BeGreaterThan(costAtRadius1000);
    }

    [Fact]
    public void TryAdvance_ReturnsEntitiesUnderMotion_OnSuccess()
    {
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(0).Build();
        var context = TestFixtures.MakeContext();
        var effect = MakeCwEffect(context, entity, totalRuneDegrees: QuarterTurn);

        var (_, entitiesUnderMotion) = effect.TryAdvance(new EventTracker());

        entitiesUnderMotion.Should().ContainSingle().Which.Should().BeSameAs(entity);
    }

    [Fact]
    public void TryAdvance_ReturnsNoEntitiesUnderMotion_OnInsufficientPower()
    {
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).Build();
        entity.Weight = 1_000_000;

        var casterEntity = new EntityBuilder()
            .WithReservoir(draw: amount => new ReservoirDraw(0, false))
            .Build();
        var caster = new EntitySet([casterEntity]);
        var context = TestFixtures.MakeContext(caster: caster);

        var effect = MakeCwEffect(context, entity, totalRuneDegrees: QuarterTurn);

        var (_, entitiesUnderMotion) = effect.TryAdvance(new EventTracker());

        entitiesUnderMotion.Should().BeEmpty();
    }
}
