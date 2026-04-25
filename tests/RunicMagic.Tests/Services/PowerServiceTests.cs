using FluentAssertions;
using RunicMagic.Tests.Builders;
using RunicMagic.World.Capabilities;
using RunicMagic.World.Execution;
using RunicMagic.World.Services;
using Xunit;

namespace RunicMagic.Tests.Services;

public class PowerServiceTests
{
    [Fact]
    public void DrawPower_DistributesWithCeilingRounding()
    {
        var drawn = new List<long>();
        var entity1 = new EntityBuilder().WithReservoir(draw: amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); }).Build();
        var entity2 = new EntityBuilder().WithReservoir(draw: amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); }).Build();
        var entity3 = new EntityBuilder().WithReservoir(draw: amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); }).Build();

        var set = new EntitySet([entity1, entity2, entity3]);
        PowerService.DrawPower(set, 10, new SpellResult());

        // ceil(10/3) = 4
        drawn.Should().AllSatisfy(d => d.Should().Be(4));
        drawn.Should().HaveCount(3);
    }

    [Fact]
    public void DrawPower_SkipsEntitiesWithoutReservoir()
    {
        var drawn = new List<long>();
        var withReservoir = new EntityBuilder()
            .WithReservoir(draw: amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); })
            .Build();
        var withoutReservoir = new EntityBuilder().Build();

        var set = new EntitySet([withReservoir, withoutReservoir]);
        PowerService.DrawPower(set, 10, new SpellResult());

        drawn.Should().HaveCount(1);
        drawn[0].Should().Be(10);
    }

    [Fact]
    public void DrawPower_EmptySet_DoesNothing()
    {
        var set = new EntitySet([]);
        var act = () => PowerService.DrawPower(set, 100, new SpellResult());
        act.Should().NotThrow();
    }

    [Fact]
    public void DrawPower_NoReservoirs_DoesNothing()
    {
        var entity = new EntityBuilder().Build();
        var set = new EntitySet([entity]);
        var act = () => PowerService.DrawPower(set, 100, new SpellResult());
        act.Should().NotThrow();
    }

    [Fact]
    public void DrawPower_EvenAmount_DistributesExactly()
    {
        var drawn = new List<long>();
        var entity1 = new EntityBuilder().WithReservoir(draw: amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); }).Build();
        var entity2 = new EntityBuilder().WithReservoir(draw: amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); }).Build();

        var set = new EntitySet([entity1, entity2]);
        PowerService.DrawPower(set, 20, new SpellResult());

        drawn.Should().AllSatisfy(d => d.Should().Be(10));
    }

    [Fact]
    public void DrawPower_EmitsPowerDrawnEvent_PerEntity()
    {
        var entity1 = new EntityBuilder().WithReservoir(draw: amount => new ReservoirDraw(amount, false)).Build();
        var entity2 = new EntityBuilder().WithReservoir(draw: amount => new ReservoirDraw(amount, false)).Build();

        var set = new EntitySet([entity1, entity2]);
        var result = new SpellResult();
        PowerService.DrawPower(set, 20, result);

        result.Events.OfType<PowerDrawnEvent>().Should().HaveCount(2);
    }

    [Fact]
    public void DrawPower_EmitsEntityDrainedEvent_WhenReservoirSignalsDrained()
    {
        var entity = new EntityBuilder()
            .WithReservoir(draw: amount => new ReservoirDraw(amount, true))
            .Build();

        var set = new EntitySet([entity]);
        var result = new SpellResult();
        PowerService.DrawPower(set, 10, result);

        result.Events.OfType<EntityDrainedEvent>().Should().ContainSingle()
            .Which.Entity.Should().Be(entity);
    }

    [Fact]
    public void DrawPower_ReturnsActualAmountDrawn()
    {
        var entity = new EntityBuilder()
            .WithReservoir(draw: amount => new ReservoirDraw(amount / 2, false))
            .Build();

        var set = new EntitySet([entity]);
        var totalDrawn = PowerService.DrawPower(set, 20, new SpellResult());

        totalDrawn.Should().Be(10);
    }

    [Fact]
    public void DrawPower_TapsLargerReservoirFirst()
    {
        var smallCalled = false;
        var small = new EntityBuilder()
            .WithReservoir(
                current: () => 500,
                draw: amount => { smallCalled = true; return new ReservoirDraw(Math.Min(amount, 500), amount >= 500); })
            .Build();
        var large = new EntityBuilder()
            .WithReservoir(
                current: () => 1500,
                draw: amount => new ReservoirDraw(Math.Min(amount, 1500), amount >= 1500))
            .Build();

        var set = new EntitySet([small, large]);
        var totalDrawn = PowerService.DrawPower(set, 1000, new SpellResult());

        totalDrawn.Should().Be(1000);
        smallCalled.Should().BeFalse("larger reservoir should satisfy the draw without touching the smaller one");
    }

    [Fact]
    public void DrawPower_SpillsToSmallerReservoirWhenLargerIsInsufficient()
    {
        var small = new EntityBuilder()
            .WithReservoir(
                current: () => 500,
                draw: amount => new ReservoirDraw(Math.Min(amount, 500), amount >= 500))
            .Build();
        var large = new EntityBuilder()
            .WithReservoir(
                current: () => 1500,
                draw: amount => new ReservoirDraw(Math.Min(amount, 1500), amount >= 1500))
            .Build();

        var set = new EntitySet([small, large]);
        var result = new SpellResult();
        var totalDrawn = PowerService.DrawPower(set, 2000, result);

        totalDrawn.Should().Be(2000);
        result.Events.OfType<EntityDrainedEvent>().Should().HaveCount(2);
    }

    [Fact]
    public void DrawPower_EntitiesAtSameLevelAreGroupedAndSplitEqually()
    {
        var drawn = new List<long>();
        var entity1 = new EntityBuilder()
            .WithReservoir(
                current: () => 1000,
                draw: amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); })
            .Build();
        var entity2 = new EntityBuilder()
            .WithReservoir(
                current: () => 1000,
                draw: amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); })
            .Build();

        var set = new EntitySet([entity1, entity2]);
        PowerService.DrawPower(set, 1000, new SpellResult());

        drawn.Should().HaveCount(2);
        drawn.Should().AllSatisfy(d => d.Should().Be(500));
    }

    [Fact]
    public void DrawPower_ReturnsPartialAmountWhenTotalCapacityIsInsufficient()
    {
        var small = new EntityBuilder()
            .WithReservoir(
                current: () => 500,
                draw: amount => new ReservoirDraw(Math.Min(amount, 500), true))
            .Build();
        var large = new EntityBuilder()
            .WithReservoir(
                current: () => 1500,
                draw: amount => new ReservoirDraw(Math.Min(amount, 1500), true))
            .Build();

        var set = new EntitySet([small, large]);
        var totalDrawn = PowerService.DrawPower(set, 3000, new SpellResult());

        totalDrawn.Should().Be(2000);
    }
}
