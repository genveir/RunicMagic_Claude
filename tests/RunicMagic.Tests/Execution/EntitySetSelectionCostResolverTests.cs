using FluentAssertions;
using RunicMagic.World;
using RunicMagic.World.Execution;
using Xunit;

namespace RunicMagic.Tests.Execution;

public class EntitySetSelectionCostResolverTests
{
    private static Entity MakeEntityWithMaxPower(long maxPower)
    {
        var entity = TestFixtures.MakeEntity();
        entity.MaxReservoir = () => maxPower;
        return entity;
    }

    private static Entity MakeCasterWithPower(long power)
    {
        var caster = TestFixtures.MakeEntity();
        caster.Reservoir = amount =>
        {
            var given = Math.Min(amount, power);
            power -= given;
            return new ReservoirDraw(given, power == 0 && given > 0);
        };
        return caster;
    }

    private static SpellContext MakeContext(Entity casterEntity, Entity? executorEntity = null)
    {
        var caster = new EntitySet([casterEntity]);
        var executor = executorEntity is not null ? new EntitySet([executorEntity]) : new EntitySet([]);
        return TestFixtures.MakeContext(caster: caster, executor: executor);
    }

    [Fact]
    public void Resolve_CasterEntityInSet_EntityCostIsExempt()
    {
        var drawn = new List<long>();
        var casterEntity = TestFixtures.MakeEntity();
        casterEntity.Reservoir = amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); };
        casterEntity.MaxReservoir = () => 5000;

        var context = MakeContext(casterEntity);
        var resolver = new EntitySetSelectionCostResolver(new FixedEntitySet(casterEntity));

        var result = resolver.Resolve(context);

        // Entity cost for caster is exempt; only breadth cost of 1 is charged
        drawn.Should().ContainSingle().Which.Should().Be(1);
        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(casterEntity);
    }

    [Fact]
    public void Resolve_ExecutorEntityInSet_EntityCostIsExempt()
    {
        var drawn = new List<long>();
        var casterEntity = TestFixtures.MakeEntity();
        casterEntity.Reservoir = amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); };

        var executorEntity = TestFixtures.MakeEntity();
        executorEntity.MaxReservoir = () => 5000;

        var context = MakeContext(casterEntity, executorEntity);
        var resolver = new EntitySetSelectionCostResolver(new FixedEntitySet(executorEntity));

        var result = resolver.Resolve(context);

        // Entity cost for executor is exempt; only breadth cost of 1 is charged
        drawn.Should().ContainSingle().Which.Should().Be(1);
        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(executorEntity);
    }

    [Fact]
    public void Resolve_SingleTaxableEntity_ExactThreshold_DrawsTwo()
    {
        var drawn = new List<long>();
        var casterEntity = TestFixtures.MakeEntity();
        casterEntity.Reservoir = amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); };

        var target = MakeEntityWithMaxPower(1000);
        var context = MakeContext(casterEntity);
        var resolver = new EntitySetSelectionCostResolver(new FixedEntitySet(target));

        resolver.Resolve(context);

        // Entity cost: ceil(1000/1000) = 1; breadth cost: 1 entity swept = 1; total = 2
        drawn.Should().ContainSingle().Which.Should().Be(2);
    }

    [Fact]
    public void Resolve_SingleTaxableEntity_SmallMaxPower_DrawsTwo()
    {
        var drawn = new List<long>();
        var casterEntity = TestFixtures.MakeEntity();
        casterEntity.Reservoir = amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); };

        var target = MakeEntityWithMaxPower(1);
        var context = MakeContext(casterEntity);
        var resolver = new EntitySetSelectionCostResolver(new FixedEntitySet(target));

        resolver.Resolve(context);

        // Entity cost: ceil(1/1000) = 1; breadth cost: 1 entity swept = 1; total = 2
        drawn.Should().ContainSingle().Which.Should().Be(2);
    }

    [Fact]
    public void Resolve_SingleTaxableEntity_JustAboveThreshold_DrawsThree()
    {
        var drawn = new List<long>();
        var casterEntity = TestFixtures.MakeEntity();
        casterEntity.Reservoir = amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); };

        var target = MakeEntityWithMaxPower(1001);
        var context = MakeContext(casterEntity);
        var resolver = new EntitySetSelectionCostResolver(new FixedEntitySet(target));

        resolver.Resolve(context);

        // Entity cost: ceil(1001/1000) = 2; breadth cost: 1 entity swept = 1; total = 3
        drawn.Should().ContainSingle().Which.Should().Be(3);
    }

    [Fact]
    public void Resolve_MultipleEntities_CostsSummed()
    {
        var drawn = new List<long>();
        var casterEntity = TestFixtures.MakeEntity();
        casterEntity.Reservoir = amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); };

        var target1 = MakeEntityWithMaxPower(1000);
        var target2 = MakeEntityWithMaxPower(2000);
        var context = MakeContext(casterEntity);
        var resolver = new EntitySetSelectionCostResolver(new FixedEntitySet(target1, target2));

        resolver.Resolve(context);

        // Entity costs: 1 + 2 = 3; breadth cost: 2 entities swept = 2; total = 5
        drawn.Should().ContainSingle().Which.Should().Be(5);
    }

    [Fact]
    public void Resolve_SufficientPower_ReturnsFullResolvedSet()
    {
        var casterEntity = MakeCasterWithPower(10);
        var target = MakeEntityWithMaxPower(1000);
        var context = MakeContext(casterEntity);
        var resolver = new EntitySetSelectionCostResolver(new FixedEntitySet(target));

        var result = resolver.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(target);
    }

    [Fact]
    public void Resolve_InsufficientPower_ReturnsEmptySet()
    {
        var casterEntity = MakeCasterWithPower(0);
        var target = MakeEntityWithMaxPower(1000);
        var context = MakeContext(casterEntity);
        var resolver = new EntitySetSelectionCostResolver(new FixedEntitySet(target));

        var result = resolver.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_InsufficientPower_AddsSelectionCostNotMetEvent()
    {
        var casterEntity = MakeCasterWithPower(0);
        var target = MakeEntityWithMaxPower(1000);
        var spellResult = new SpellResult();
        var caster = new EntitySet([casterEntity]);
        var context = new SpellContext(caster, new EntitySet([]), new WorldModel(), spellResult);
        var resolver = new EntitySetSelectionCostResolver(new FixedEntitySet(target));

        resolver.Resolve(context);

        // Entity cost: 1; breadth cost: 1; total required = 2
        spellResult.Events.OfType<SelectionCostNotMetEvent>().Should().ContainSingle()
            .Which.Should().Match<SelectionCostNotMetEvent>(e => e.Required == 2 && e.Drawn == 0);
    }

    [Fact]
    public void Resolve_MixedSet_OnlyNonExemptEntitiesAreTaxed()
    {
        var drawn = new List<long>();
        var casterEntity = TestFixtures.MakeEntity();
        casterEntity.Reservoir = amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); };
        casterEntity.MaxReservoir = () => 5000;

        var target = MakeEntityWithMaxPower(1000);
        var context = MakeContext(casterEntity);
        var resolver = new EntitySetSelectionCostResolver(new FixedEntitySet(casterEntity, target));

        var result = resolver.Resolve(context);

        // Entity cost: target only = 1 (caster exempt); breadth cost: 2 entities swept = 2; total = 3
        drawn.Should().ContainSingle().Which.Should().Be(3);
        result.Entities.Should().HaveCount(2);
    }

    [Fact]
    public void Resolve_BreadthCost_EachEntitySweptCostsOne()
    {
        var drawn = new List<long>();
        var casterEntity = TestFixtures.MakeEntity();
        casterEntity.Reservoir = amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); };

        var targets = Enumerable.Range(0, 3).Select(_ => TestFixtures.MakeEntity()).ToArray();
        var context = MakeContext(casterEntity);
        var resolver = new EntitySetSelectionCostResolver(new FixedEntitySet(targets));

        resolver.Resolve(context);

        // 3 entities swept, no entity cost (no MaxReservoir), breadth cost = 3
        drawn.Should().ContainSingle().Which.Should().Be(3);
    }

    [Fact]
    public void Resolve_BreadthCost_Exactly10EntitiesSwept_Charges10()
    {
        var drawn = new List<long>();
        var casterEntity = TestFixtures.MakeEntity();
        casterEntity.Reservoir = amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); };

        var targets = Enumerable.Range(0, 10).Select(_ => TestFixtures.MakeEntity()).ToArray();
        var context = MakeContext(casterEntity);
        var resolver = new EntitySetSelectionCostResolver(new FixedEntitySet(targets));

        resolver.Resolve(context);

        drawn.Should().ContainSingle().Which.Should().Be(10);
    }

    [Fact]
    public void Resolve_BreadthCost_AddedToEntitySelectionCost()
    {
        var drawn = new List<long>();
        var casterEntity = TestFixtures.MakeEntity();
        casterEntity.Reservoir = amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); };

        var targets = Enumerable.Range(0, 10).Select(_ => MakeEntityWithMaxPower(1000)).ToArray();
        var context = MakeContext(casterEntity);
        var resolver = new EntitySetSelectionCostResolver(new FixedEntitySet(targets));

        resolver.Resolve(context);

        // Entity cost: 10 * ceil(1000/1000) = 10; breadth cost: 10 entities swept = 10; total = 20
        drawn.Should().ContainSingle().Which.Should().Be(20);
    }

    [Fact]
    public void Resolve_BreadthCost_WindowClosedAfterResolve()
    {
        var casterEntity = TestFixtures.MakeEntity();
        casterEntity.Reservoir = amount => new ReservoirDraw(amount, false);
        var context = MakeContext(casterEntity);
        var resolver = new EntitySetSelectionCostResolver(new FixedEntitySet(TestFixtures.MakeEntity()));

        resolver.Resolve(context);

        context.EntityResolutionCount.Should().BeNull();
    }

    [Fact]
    public void Resolve_BreadthCost_AppliedEvenWhenFinalSetIsEmpty()
    {
        var drawn = new List<long>();
        var casterEntity = TestFixtures.MakeEntity();
        casterEntity.Reservoir = amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); };

        // 10 entities swept, but insufficient power to pay — still charges breadth upfront alongside entity cost
        var casterWithLimitedPower = MakeCasterWithPower(0);
        var targets = Enumerable.Range(0, 10).Select(_ => TestFixtures.MakeEntity()).ToArray();
        var context = MakeContext(casterWithLimitedPower);
        var resolver = new EntitySetSelectionCostResolver(new FixedEntitySet(targets));

        var result = resolver.Resolve(context);

        result.Entities.Should().BeEmpty();
        context.EntityResolutionCount.Should().BeNull();
    }
}
