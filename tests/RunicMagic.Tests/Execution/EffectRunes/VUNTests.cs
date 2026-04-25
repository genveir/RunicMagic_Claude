using FluentAssertions;
using RunicMagic.Tests.Builders;
using RunicMagic.World.Capabilities;
using RunicMagic.World.Execution;
using RunicMagic.World.Runes.EffectRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.EffectRunes;

public class VUNTests
{
    [Fact]
    public void Execute_PushesEntityAwayFromOrigin()
    {
        var entity = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(1).Build();
        var vun = new VUN(
            toMove: new FixedEntitySet(entity),
            howFar: new FixedNumber(500),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext();

        vun.Execute(context);

        entity.Location.X.Should().Be(1500);
        entity.Location.Y.Should().Be(0);
    }

    [Fact]
    public void Execute_NullVector_EntityStillMoves()
    {
        var entity = new EntityBuilder().WithLocation(x: 0, y: 0).WithWeight(1).Build();
        var vun = new VUN(
            toMove: new FixedEntitySet(entity),
            howFar: new FixedNumber(100),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext();

        vun.Execute(context);

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

        // 1000mm (1m) × 1000g (1kg) / 1_000_000 = 1 power per kg·m
        var target = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(1000).Build();
        var vun = new VUN(
            toMove: new FixedEntitySet(target),
            howFar: new FixedNumber(1000),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext(caster: caster);

        vun.Execute(context);

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

        // cost = 2000mm * 1000g / 1_000_000 = 2 power; executor provides 1, caster covers remaining 1
        var target = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(1000).Build();
        var vun = new VUN(
            toMove: new FixedEntitySet(target),
            howFar: new FixedNumber(2000),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext(caster: caster, executor: executor);

        vun.Execute(context);

        executorDrawn.Should().ContainSingle().Which.Should().Be(2);
        casterDrawn.Should().ContainSingle().Which.Should().Be(1);
    }

    [Fact]
    public void Execute_MultipleEntities_AllMoveAwayFromOrigin()
    {
        var entity1 = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(1).Build();
        var entity2 = new EntityBuilder().WithLocation(x: 0, y: 1000).WithWeight(1).Build();
        var vun = new VUN(
            toMove: new FixedEntitySet(entity1, entity2),
            howFar: new FixedNumber(200),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext();

        vun.Execute(context);

        entity1.Location.X.Should().Be(1200);
        entity1.Location.Y.Should().Be(0);
        entity2.Location.X.Should().Be(0);
        entity2.Location.Y.Should().Be(1200);
    }

    [Fact]
    public void Execute_EmitsEntityPushedEvent_PerEntity()
    {
        var entity1 = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(1).Build();
        var entity2 = new EntityBuilder().WithLocation(x: 0, y: 1000).WithWeight(1).Build();
        var result = new SpellResult();
        var vun = new VUN(
            toMove: new FixedEntitySet(entity1, entity2),
            howFar: new FixedNumber(200),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext(result: result);

        vun.Execute(context);

        result.Events.OfType<EntityPushedEvent>().Should().HaveCount(2);
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
        var vun = new VUN(
            toMove: new FixedEntitySet(entity),
            howFar: new FixedNumber(1000),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext(caster: caster, result: result);

        vun.Execute(context);

        entity.Location.X.Should().Be(originalX);
        result.Events.OfType<EffectNotFiredEvent>().Should().ContainSingle()
            .Which.Effect.Should().Be("VUN");
    }

    [Fact]
    public void Execute_EmptySet_DrawsNoPower()
    {
        var drawn = new List<long>();
        var casterEntity = new EntityBuilder()
            .WithReservoir(draw: amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); })
            .Build();
        var caster = new EntitySet([casterEntity]);

        var vun = new VUN(
            toMove: new FixedEntitySet(),
            howFar: new FixedNumber(500),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext(caster: caster);

        vun.Execute(context);

        drawn.Should().BeEmpty();
    }
}
