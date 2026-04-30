using FluentAssertions;
using RunicMagic.Controller.Services;
using RunicMagic.Tests.Builders;
using RunicMagic.World.Capabilities;
using RunicMagic.World.Execution;
using Xunit;

namespace RunicMagic.Tests.Execution;

public class EntitySetSelectionCostResolverTests
{
    // Helper: builds a caster EntitySet that tracks how much power is drawn.
    private static (EntitySet caster, List<long> drawn) MakeTrackingCaster()
    {
        var drawnAmounts = new List<long>();
        var entity = new EntityBuilder()
            .WithReservoir(draw: amount =>
            {
                drawnAmounts.Add(amount);
                return new ReservoirDraw(amount, false);
            })
            .Build();
        return (new EntitySet([entity]), drawnAmounts);
    }

    [Fact]
    public void Resolve_EmptyInnerSet_ReturnsThatSet()
    {
        var inner = new FixedEntitySet();
        var resolver = new EntitySetSelectionCostResolver(inner);
        var context = TestFixtures.MakeContext();

        var result = resolver.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_SufficientPower_ReturnsResolvedSet()
    {
        var target = new EntityBuilder().Build();
        var inner = new FixedEntitySet(target);
        var resolver = new EntitySetSelectionCostResolver(inner);
        var (caster, _) = MakeTrackingCaster();
        var context = TestFixtures.MakeContext(caster: caster);

        var result = resolver.Resolve(context);

        result.Entities.Should().Contain(target);
    }

    [Fact]
    public void Resolve_InsufficientPower_ReturnsEmptySet()
    {
        var target = new EntityBuilder()
            .WithReservoir(max: () => 1000)
            .Build();
        var inner = new FixedEntitySet(target);
        var resolver = new EntitySetSelectionCostResolver(inner);
        var context = TestFixtures.MakeContext(); // no power sources

        var result = resolver.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_InsufficientPower_EmitsSelectionCostNotMetEvent()
    {
        var target = new EntityBuilder()
            .WithReservoir(max: () => 1000)
            .Build();
        var inner = new FixedEntitySet(target);
        var resolver = new EntitySetSelectionCostResolver(inner);
        var result = new EventTracker();
        var context = TestFixtures.MakeContext(result: result);

        resolver.Resolve(context);

        result.WorldEvents.OfType<SelectionCostNotMetEvent>().Should().ContainSingle();
    }

    [Fact]
    public void Resolve_PartialPower_EventHasCorrectRequiredAndDrawn()
    {
        var target = new EntityBuilder()
            .WithReservoir(max: () => 2000)
            .Build();
        var inner = new FixedEntitySet(target);
        var resolver = new EntitySetSelectionCostResolver(inner);

        var casterEntity = new EntityBuilder()
            .WithReservoir(draw: amount => new ReservoirDraw(1, false))
            .Build();
        var caster = new EntitySet([casterEntity]);
        var result = new EventTracker();
        var context = TestFixtures.MakeContext(caster: caster, result: result);

        resolver.Resolve(context);

        // final set cost = ceil(2000/1_000_000_000) = 1; breadth = 1 × 1_000_000; total required = 1_000_001; drawn = 1
        var evt = result.WorldEvents.OfType<SelectionCostNotMetEvent>().Single();
        evt.Required.Should().Be(1_000_001);
        evt.Drawn.Should().Be(1);
    }

    [Fact]
    public void FinalSetCost_EntityWithNoReservoir_ContributesZero()
    {
        var target = new EntityBuilder().Build(); // no reservoir
        var inner = new FixedEntitySet(target);
        var resolver = new EntitySetSelectionCostResolver(inner);
        var (caster, drawn) = MakeTrackingCaster();
        var context = TestFixtures.MakeContext(caster: caster);

        resolver.Resolve(context);

        // final set cost = 0 (no reservoir); breadth = 1 × 1_000_000; total = 1_000_000
        drawn.Should().ContainSingle().Which.Should().Be(1_000_000);
    }

    [Fact]
    public void FinalSetCost_EntityWithReservoir_ContributesCeilOfMaxPowerDividedBy1000000000()
    {
        var target = new EntityBuilder()
            .WithReservoir(max: () => 3_000_000_000)
            .Build();
        var inner = new FixedEntitySet(target);
        var resolver = new EntitySetSelectionCostResolver(inner);
        var (caster, drawn) = MakeTrackingCaster();
        var context = TestFixtures.MakeContext(caster: caster);

        resolver.Resolve(context);

        // final set cost = 3_000_000_000/1_000_000_000 = 3; breadth = 1 × 1_000_000; total = 1_000_003
        drawn.Should().ContainSingle().Which.Should().Be(1_000_003);
    }

    [Fact]
    public void FinalSetCost_PartialMaxPower_RoundsUp()
    {
        var target = new EntityBuilder()
            .WithReservoir(max: () => 1_000_000_001)
            .Build();
        var inner = new FixedEntitySet(target);
        var resolver = new EntitySetSelectionCostResolver(inner);
        var (caster, drawn) = MakeTrackingCaster();
        var context = TestFixtures.MakeContext(caster: caster);

        resolver.Resolve(context);

        // final set cost = ceil(1_000_000_001/1_000_000_000) = 2; breadth = 1 × 1_000_000; total = 1_000_002
        drawn.Should().ContainSingle().Which.Should().Be(1_000_002);
    }

    [Fact]
    public void FinalSetCost_CasterEntityInResolvedSet_IsExempt()
    {
        var casterEntity = new EntityBuilder()
            .WithReservoir(max: () => 5000)
            .Build();
        var caster = new EntitySet([casterEntity]);

        // a second entity provides power so we can measure the draw amount
        var (powerCaster, drawn) = MakeTrackingCaster();
        var combinedCaster = new EntitySet([casterEntity, powerCaster.Entities[0]]);

        var inner = new FixedEntitySet(casterEntity); // resolved set includes the caster entity
        var resolver = new EntitySetSelectionCostResolver(inner);
        var context = TestFixtures.MakeContext(caster: combinedCaster);

        resolver.Resolve(context);

        // casterEntity is exempt → final set cost = 0; breadth = 1 × 1_000_000; total = 1_000_000
        // two caster entities at the same charge level → each asked for ceil(1_000_000 / 2) = 500_000
        drawn.Should().ContainSingle().Which.Should().Be(500_000);
    }

    [Fact]
    public void FinalSetCost_ExecutorEntityInResolvedSet_IsExempt()
    {
        // executor entity cannot supply power (draw returns 0), so any cost is borne by the caster
        var executorEntity = new EntityBuilder()
            .WithReservoir(max: () => 5000, draw: amount => new ReservoirDraw(0, false))
            .Build();
        var executor = new EntitySet([executorEntity]);

        var (caster, drawn) = MakeTrackingCaster();

        var inner = new FixedEntitySet(executorEntity); // resolved set includes the executor entity
        var resolver = new EntitySetSelectionCostResolver(inner);
        var context = TestFixtures.MakeContext(caster: caster, executor: executor);

        resolver.Resolve(context);

        // executorEntity is exempt → final set cost = 0; breadth = 1 × 1_000_000; total = 1_000_000
        drawn.Should().ContainSingle().Which.Should().Be(1_000_000);
    }

    [Fact]
    public void BreadthCost_EqualsCountOfUniqueEntitiesResolvedByInner()
    {
        var e1 = new EntityBuilder().Build();
        var e2 = new EntityBuilder().Build();
        var e3 = new EntityBuilder().Build();
        var inner = new FixedEntitySet(e1, e2, e3);
        var resolver = new EntitySetSelectionCostResolver(inner);
        var (caster, drawn) = MakeTrackingCaster();
        var context = TestFixtures.MakeContext(caster: caster);

        resolver.Resolve(context);

        // all 3 entities have no reservoir → final set cost = 0; breadth = 3 × 1_000_000; total = 3_000_000
        drawn.Should().ContainSingle().Which.Should().Be(3_000_000);
    }

    [Fact]
    public void Resolve_DoesNotPollutOuterResolutionWindow()
    {
        var target = new EntityBuilder().Build();
        var inner = new FixedEntitySet(target);
        var resolver = new EntitySetSelectionCostResolver(inner);
        var (caster, _) = MakeTrackingCaster();
        var context = TestFixtures.MakeContext(caster: caster);
        context.OpenResolutionWindow();

        resolver.Resolve(context);

        // the inner resolution window was opened and closed; the outer window should be untouched
        context.EntityResolutionCount.Should().BeEmpty();
        context.CloseResolutionWindow();
    }
}
