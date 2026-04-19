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
    public void Resolve_NoTaxableEntities_ReturnsSetWithoutDrawingPower()
    {
        var drawn = new List<long>();
        var casterEntity = TestFixtures.MakeEntity();
        casterEntity.Reservoir = amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); };

        var target = TestFixtures.MakeEntity();
        var context = MakeContext(casterEntity);
        var resolver = new EntitySetSelectionCostResolver(new FixedEntitySet(target));

        resolver.Resolve(context);

        drawn.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_CasterEntityInSet_IsNotTaxed()
    {
        var drawn = new List<long>();
        var casterEntity = TestFixtures.MakeEntity();
        casterEntity.Reservoir = amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); };
        casterEntity.MaxReservoir = () => 5000;

        var context = MakeContext(casterEntity);
        var resolver = new EntitySetSelectionCostResolver(new FixedEntitySet(casterEntity));

        var result = resolver.Resolve(context);

        drawn.Should().BeEmpty();
        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(casterEntity);
    }

    [Fact]
    public void Resolve_ExecutorEntityInSet_IsNotTaxed()
    {
        var drawn = new List<long>();
        var casterEntity = TestFixtures.MakeEntity();
        casterEntity.Reservoir = amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); };

        var executorEntity = TestFixtures.MakeEntity();
        executorEntity.MaxReservoir = () => 5000;

        var context = MakeContext(casterEntity, executorEntity);
        var resolver = new EntitySetSelectionCostResolver(new FixedEntitySet(executorEntity));

        var result = resolver.Resolve(context);

        drawn.Should().BeEmpty();
        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(executorEntity);
    }

    [Fact]
    public void Resolve_SingleTaxableEntity_ExactThreshold_DrawsOne()
    {
        var drawn = new List<long>();
        var casterEntity = TestFixtures.MakeEntity();
        casterEntity.Reservoir = amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); };

        var target = MakeEntityWithMaxPower(1000);
        var context = MakeContext(casterEntity);
        var resolver = new EntitySetSelectionCostResolver(new FixedEntitySet(target));

        resolver.Resolve(context);

        drawn.Should().ContainSingle().Which.Should().Be(1);
    }

    [Fact]
    public void Resolve_SingleTaxableEntity_SmallMaxPower_RoundsUpToOne()
    {
        var drawn = new List<long>();
        var casterEntity = TestFixtures.MakeEntity();
        casterEntity.Reservoir = amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); };

        var target = MakeEntityWithMaxPower(1);
        var context = MakeContext(casterEntity);
        var resolver = new EntitySetSelectionCostResolver(new FixedEntitySet(target));

        resolver.Resolve(context);

        drawn.Should().ContainSingle().Which.Should().Be(1);
    }

    [Fact]
    public void Resolve_SingleTaxableEntity_JustAboveThreshold_DrawsTwo()
    {
        var drawn = new List<long>();
        var casterEntity = TestFixtures.MakeEntity();
        casterEntity.Reservoir = amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); };

        var target = MakeEntityWithMaxPower(1001);
        var context = MakeContext(casterEntity);
        var resolver = new EntitySetSelectionCostResolver(new FixedEntitySet(target));

        resolver.Resolve(context);

        drawn.Should().ContainSingle().Which.Should().Be(2);
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

        drawn.Should().ContainSingle().Which.Should().Be(3);
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

        spellResult.Events.OfType<SelectionCostNotMetEvent>().Should().ContainSingle()
            .Which.Should().Match<SelectionCostNotMetEvent>(e => e.Required == 1 && e.Drawn == 0);
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

        drawn.Should().ContainSingle().Which.Should().Be(1);
        result.Entities.Should().HaveCount(2);
    }
}
