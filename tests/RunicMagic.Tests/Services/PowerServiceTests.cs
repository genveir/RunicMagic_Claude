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

    [Fact]
    public void FillPower_DistributesWithCeilingRounding()
    {
        var filled = new List<long>();
        var entity1 = new EntityBuilder().WithReservoir(fill: amount => { filled.Add(amount); return new ReservoirFill(amount, false); }).Build();
        var entity2 = new EntityBuilder().WithReservoir(fill: amount => { filled.Add(amount); return new ReservoirFill(amount, false); }).Build();
        var entity3 = new EntityBuilder().WithReservoir(fill: amount => { filled.Add(amount); return new ReservoirFill(amount, false); }).Build();

        var set = new EntitySet([entity1, entity2, entity3]);
        PowerService.FillPower(set, 10, new SpellResult());

        // ceil(10/3) = 4
        filled.Should().AllSatisfy(f => f.Should().Be(4));
        filled.Should().HaveCount(3);
    }

    [Fact]
    public void FillPower_SkipsEntitiesWithoutReservoir()
    {
        var filled = new List<long>();
        var withReservoir = new EntityBuilder()
            .WithReservoir(fill: amount => { filled.Add(amount); return new ReservoirFill(amount, false); })
            .Build();
        var withoutReservoir = new EntityBuilder().Build();

        var set = new EntitySet([withReservoir, withoutReservoir]);
        PowerService.FillPower(set, 10, new SpellResult());

        filled.Should().HaveCount(1);
        filled[0].Should().Be(10);
    }

    [Fact]
    public void FillPower_EmptySet_DoesNothing()
    {
        var set = new EntitySet([]);
        var act = () => PowerService.FillPower(set, 100, new SpellResult());
        act.Should().NotThrow();
    }

    [Fact]
    public void FillPower_NoReservoirs_DoesNothing()
    {
        var entity = new EntityBuilder().Build();
        var set = new EntitySet([entity]);
        var act = () => PowerService.FillPower(set, 100, new SpellResult());
        act.Should().NotThrow();
    }

    [Fact]
    public void FillPower_EvenAmount_DistributesExactly()
    {
        var filled = new List<long>();
        var entity1 = new EntityBuilder().WithReservoir(fill: amount => { filled.Add(amount); return new ReservoirFill(amount, false); }).Build();
        var entity2 = new EntityBuilder().WithReservoir(fill: amount => { filled.Add(amount); return new ReservoirFill(amount, false); }).Build();

        var set = new EntitySet([entity1, entity2]);
        PowerService.FillPower(set, 20, new SpellResult());

        filled.Should().AllSatisfy(f => f.Should().Be(10));
    }

    [Fact]
    public void FillPower_EmitsPowerFilledEvent_PerEntity()
    {
        var entity1 = new EntityBuilder().WithReservoir(fill: amount => new ReservoirFill(amount, false)).Build();
        var entity2 = new EntityBuilder().WithReservoir(fill: amount => new ReservoirFill(amount, false)).Build();

        var set = new EntitySet([entity1, entity2]);
        var result = new SpellResult();
        PowerService.FillPower(set, 20, result);

        result.Events.OfType<PowerFilledEvent>().Should().HaveCount(2);
    }

    [Fact]
    public void FillPower_EmitsEntityFullEvent_WhenReservoirSignalsFull()
    {
        var entity = new EntityBuilder()
            .WithReservoir(fill: amount => new ReservoirFill(amount, true))
            .Build();

        var set = new EntitySet([entity]);
        var result = new SpellResult();
        PowerService.FillPower(set, 10, result);

        result.Events.OfType<EntityFullEvent>().Should().ContainSingle()
            .Which.Entity.Should().Be(entity);
    }

    [Fact]
    public void FillPower_ReturnsActualAmountFilled()
    {
        var entity = new EntityBuilder()
            .WithReservoir(fill: amount => new ReservoirFill(amount / 2, false))
            .Build();

        var set = new EntitySet([entity]);
        var totalFilled = PowerService.FillPower(set, 20, new SpellResult());

        totalFilled.Should().Be(10);
    }

    [Fact]
    public void FillPower_TapsEmptierReservoirFirst()
    {
        var fullCalled = false;
        var empty = new EntityBuilder()
            .WithReservoir(
                current: () => 500,
                fill: amount => new ReservoirFill(Math.Min(amount, 1000), amount >= 1000))
            .Build();
        var full = new EntityBuilder()
            .WithReservoir(
                current: () => 1500,
                fill: amount => { fullCalled = true; return new ReservoirFill(Math.Min(amount, 500), amount >= 500); })
            .Build();

        var set = new EntitySet([empty, full]);
        var totalFilled = PowerService.FillPower(set, 500, new SpellResult());

        totalFilled.Should().Be(500);
        fullCalled.Should().BeFalse("emptier reservoir should absorb the fill without touching the fuller one");
    }

    [Fact]
    public void FillPower_SpillsToFullerReservoirWhenEmptierIsInsufficient()
    {
        var empty = new EntityBuilder()
            .WithReservoir(
                current: () => 500,
                fill: amount => new ReservoirFill(Math.Min(amount, 1000), amount >= 1000))
            .Build();
        var full = new EntityBuilder()
            .WithReservoir(
                current: () => 1500,
                fill: amount => new ReservoirFill(Math.Min(amount, 500), amount >= 500))
            .Build();

        var set = new EntitySet([empty, full]);
        var result = new SpellResult();
        var totalFilled = PowerService.FillPower(set, 1500, result);

        totalFilled.Should().Be(1500);
        result.Events.OfType<EntityFullEvent>().Should().HaveCount(2);
    }

    [Fact]
    public void FillPower_EntitiesAtSameLevelAreGroupedAndSplitEqually()
    {
        var filled = new List<long>();
        var entity1 = new EntityBuilder()
            .WithReservoir(
                current: () => 1000,
                fill: amount => { filled.Add(amount); return new ReservoirFill(amount, false); })
            .Build();
        var entity2 = new EntityBuilder()
            .WithReservoir(
                current: () => 1000,
                fill: amount => { filled.Add(amount); return new ReservoirFill(amount, false); })
            .Build();

        var set = new EntitySet([entity1, entity2]);
        PowerService.FillPower(set, 1000, new SpellResult());

        filled.Should().HaveCount(2);
        filled.Should().AllSatisfy(f => f.Should().Be(500));
    }

    [Fact]
    public void FillPower_ReturnsPartialAmountWhenTotalCapacityIsInsufficient()
    {
        var empty = new EntityBuilder()
            .WithReservoir(
                current: () => 500,
                fill: amount => new ReservoirFill(Math.Min(amount, 1000), true))
            .Build();
        var full = new EntityBuilder()
            .WithReservoir(
                current: () => 1500,
                fill: amount => new ReservoirFill(Math.Min(amount, 500), true))
            .Build();

        var set = new EntitySet([empty, full]);
        var totalFilled = PowerService.FillPower(set, 3000, new SpellResult());

        totalFilled.Should().Be(1500);
    }
}
