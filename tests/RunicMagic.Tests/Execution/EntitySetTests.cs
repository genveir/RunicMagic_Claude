using FluentAssertions;
using RunicMagic.World.Execution;
using Xunit;

namespace RunicMagic.Tests.Execution;

public class EntitySetTests
{
    [Fact]
    public void DrawPower_DistributesWithCeilingRounding()
    {
        var drawn = new List<long>();
        var entity1 = TestFixtures.MakeEntity();
        var entity2 = TestFixtures.MakeEntity();
        var entity3 = TestFixtures.MakeEntity();
        entity1.Reservoir = amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); };
        entity2.Reservoir = amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); };
        entity3.Reservoir = amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); };

        var set = new EntitySet([entity1, entity2, entity3]);
        set.DrawPower(10, new SpellResult());

        // ceil(10/3) = 4
        drawn.Should().AllSatisfy(d => d.Should().Be(4));
        drawn.Should().HaveCount(3);
    }

    [Fact]
    public void DrawPower_SkipsEntitiesWithoutReservoir()
    {
        var drawn = new List<long>();
        var withReservoir = TestFixtures.MakeEntity();
        var withoutReservoir = TestFixtures.MakeEntity();
        withReservoir.Reservoir = amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); };

        var set = new EntitySet([withReservoir, withoutReservoir]);
        set.DrawPower(10, new SpellResult());

        drawn.Should().HaveCount(1);
        drawn[0].Should().Be(10);
    }

    [Fact]
    public void DrawPower_EmptySet_DoesNothing()
    {
        var set = new EntitySet([]);
        var act = () => set.DrawPower(100, new SpellResult());
        act.Should().NotThrow();
    }

    [Fact]
    public void DrawPower_NoReservoirs_DoesNothing()
    {
        var entity = TestFixtures.MakeEntity();
        var set = new EntitySet([entity]);
        var act = () => set.DrawPower(100, new SpellResult());
        act.Should().NotThrow();
    }

    [Fact]
    public void DrawPower_EvenAmount_DistributesExactly()
    {
        var drawn = new List<long>();
        var entity1 = TestFixtures.MakeEntity();
        var entity2 = TestFixtures.MakeEntity();
        entity1.Reservoir = amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); };
        entity2.Reservoir = amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); };

        var set = new EntitySet([entity1, entity2]);
        set.DrawPower(20, new SpellResult());

        drawn.Should().AllSatisfy(d => d.Should().Be(10));
    }

    [Fact]
    public void DrawPower_EmitsPowerDrawnEvent_PerEntity()
    {
        var entity1 = TestFixtures.MakeEntity();
        var entity2 = TestFixtures.MakeEntity();
        entity1.Reservoir = amount => new ReservoirDraw(amount, false);
        entity2.Reservoir = amount => new ReservoirDraw(amount, false);

        var set = new EntitySet([entity1, entity2]);
        var result = new SpellResult();
        set.DrawPower(20, result);

        result.Events.OfType<PowerDrawnEvent>().Should().HaveCount(2);
    }

    [Fact]
    public void DrawPower_EmitsEntityDrainedEvent_WhenReservoirSignalsDrained()
    {
        var entity = TestFixtures.MakeEntity();
        entity.Reservoir = amount => new ReservoirDraw(amount, true);

        var set = new EntitySet([entity]);
        var result = new SpellResult();
        set.DrawPower(10, result);

        result.Events.OfType<EntityDrainedEvent>().Should().ContainSingle()
            .Which.Entity.Should().Be(entity);
    }

    [Fact]
    public void DrawPower_ReturnsActualAmountDrawn()
    {
        var entity = TestFixtures.MakeEntity();
        entity.Reservoir = amount => new ReservoirDraw(amount / 2, false);

        var set = new EntitySet([entity]);
        var totalDrawn = set.DrawPower(20, new SpellResult());

        totalDrawn.Should().Be(10);
    }
}
