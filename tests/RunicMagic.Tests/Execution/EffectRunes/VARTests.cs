using FluentAssertions;
using RunicMagic.Tests.Builders;
using RunicMagic.World.Capabilities;
using RunicMagic.World.Execution;
using RunicMagic.World.Runes.EffectRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.EffectRunes;

public class VARTests
{
    [Fact]
    public void Execute_PullsEntityTowardsOrigin()
    {
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(1).Build();
        var var_ = new VAR(
            toMove: new FixedEntitySet(entity),
            howFar: new FixedNumber(500),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext();

        var_.Execute(context);

        entity.Location.X.Should().Be(500);
        entity.Location.Y.Should().Be(0);
    }

    [Fact]
    public void Execute_NullVector_EntityStillMoves()
    {
        var entity = new EntityBuilder().WithLocation(x: 0, y: 0).WithWeight(1).Build();
        var var_ = new VAR(
            toMove: new FixedEntitySet(entity),
            howFar: new FixedNumber(100),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext();

        var_.Execute(context);

        var displaced = entity.Location.X != 0 || entity.Location.Y != 0;
        displaced.Should().BeTrue();
    }

    [Fact]
    public void Execute_DrawsPowerFromCaster()
    {
        var drawn = new List<long>();
        var casterEntity = new EntityBuilder()
            .WithReservoir(draw: amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); })
            .Build();
        var caster = new EntitySet([casterEntity]);

        var target = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(1000).Build();
        var var_ = new VAR(
            toMove: new FixedEntitySet(target),
            howFar: new FixedNumber(1000),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext(caster: caster);

        var_.Execute(context);

        drawn.Should().ContainSingle().Which.Should().Be(1);
    }

    [Fact]
    public void Execute_DrawsFromExecutorFirst_ThenCaster()
    {
        var executorDrawn = new List<long>();
        var casterDrawn = new List<long>();

        var executorEntity = new EntityBuilder()
            .WithReservoir(draw: amount => { executorDrawn.Add(amount); return new ReservoirDraw(amount / 2, false); })
            .Build();
        var executor = new EntitySet([executorEntity]);

        var casterEntity = new EntityBuilder()
            .WithReservoir(draw: amount => { casterDrawn.Add(amount); return new ReservoirDraw(amount, false); })
            .Build();
        var caster = new EntitySet([casterEntity]);

        var target = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(1000).Build();
        var var_ = new VAR(
            toMove: new FixedEntitySet(target),
            howFar: new FixedNumber(2000),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext(caster: caster, executor: executor);

        var_.Execute(context);

        executorDrawn.Should().ContainSingle().Which.Should().Be(2);
        casterDrawn.Should().ContainSingle().Which.Should().Be(1);
    }

    [Fact]
    public void Execute_MultipleEntities_AllMoveTowardsOrigin()
    {
        var entity1 = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(1).Build();
        var entity2 = new EntityBuilder().WithLocation(x: 0, y: 1000).WithWeight(1).Build();
        var var_ = new VAR(
            toMove: new FixedEntitySet(entity1, entity2),
            howFar: new FixedNumber(200),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext();

        var_.Execute(context);

        entity1.Location.X.Should().Be(800);
        entity1.Location.Y.Should().Be(0);
        entity2.Location.X.Should().Be(0);
        entity2.Location.Y.Should().Be(800);
    }

    [Fact]
    public void Execute_EmitsEntityPulledEvent_PerEntity()
    {
        var entity1 = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(1).Build();
        var entity2 = new EntityBuilder().WithLocation(x: 0, y: 1000).WithWeight(1).Build();
        var result = new SpellResult();
        var var_ = new VAR(
            toMove: new FixedEntitySet(entity1, entity2),
            howFar: new FixedNumber(200),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext(result: result);

        var_.Execute(context);

        result.Events.OfType<EntityPulledEvent>().Should().HaveCount(2);
    }

    [Fact]
    public void Execute_InsufficientPower_DoesNotFireAndEmitsEvent()
    {
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(1000).Build();
        var originalX = entity.Location.X;

        var casterEntity = new EntityBuilder()
            .WithReservoir(draw: amount => new ReservoirDraw(0, false))
            .Build();
        var caster = new EntitySet([casterEntity]);

        var result = new SpellResult();
        var var_ = new VAR(
            toMove: new FixedEntitySet(entity),
            howFar: new FixedNumber(1000),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext(caster: caster, result: result);

        var_.Execute(context);

        entity.Location.X.Should().Be(originalX);
        result.Events.OfType<EffectNotFiredEvent>().Should().ContainSingle()
            .Which.Effect.Should().Be("VAR");
    }

    [Fact]
    public void Execute_EmptySet_DrawsNoPower()
    {
        var drawn = new List<long>();
        var casterEntity = new EntityBuilder()
            .WithReservoir(draw: amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); })
            .Build();
        var caster = new EntitySet([casterEntity]);

        var var_ = new VAR(
            toMove: new FixedEntitySet(),
            howFar: new FixedNumber(500),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext(caster: caster);

        var_.Execute(context);

        drawn.Should().BeEmpty();
    }
}
