using FluentAssertions;
using RunicMagic.World.Execution;
using RunicMagic.World.Runes.EffectRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.EffectRunes;

public class VUNTests
{
    [Fact]
    public void Execute_PushesEntityAwayFromOrigin()
    {
        var entity = TestFixtures.MakeEntity(x: 1000, y: 0, weight: 1);
        var vun = new VUN(
            toMove: new FixedEntitySet(entity),
            howFar: new FixedNumber(500),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext();

        vun.Execute(context);

        entity.X.Should().Be(1500);
        entity.Y.Should().Be(0);
    }

    [Fact]
    public void Execute_NullVector_EntityStillMoves()
    {
        var entity = TestFixtures.MakeEntity(x: 0, y: 0, weight: 1);
        var vun = new VUN(
            toMove: new FixedEntitySet(entity),
            howFar: new FixedNumber(100),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext();

        vun.Execute(context);

        var displaced = entity.X != 0 || entity.Y != 0;
        displaced.Should().BeTrue();
    }

    [Fact]
    public void Execute_DrawsPowerFromCaster()
    {
        var drawn = new List<long>();
        var casterEntity = TestFixtures.MakeEntity();
        casterEntity.Reservoir = amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); };
        var caster = new EntitySet([casterEntity]);

        // 1000mm (1m) × 1000g (1kg) / 1_000_000 = 1 power per kg·m
        var target = TestFixtures.MakeEntity(x: 1000, y: 0, weight: 1000);
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

        var executorEntity = TestFixtures.MakeEntity();
        executorEntity.Reservoir = amount => { executorDrawn.Add(amount); return new ReservoirDraw(amount / 2, false); };
        var executor = new EntitySet([executorEntity]);

        var casterEntity = TestFixtures.MakeEntity();
        casterEntity.Reservoir = amount => { casterDrawn.Add(amount); return new ReservoirDraw(amount, false); };
        var caster = new EntitySet([casterEntity]);

        // cost = 2000mm * 1000g / 1_000_000 = 2 power; executor provides 1, caster covers remaining 1
        var target = TestFixtures.MakeEntity(x: 1000, y: 0, weight: 1000);
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
        var entity1 = TestFixtures.MakeEntity(x: 1000, y: 0, weight: 1);
        var entity2 = TestFixtures.MakeEntity(x: 0, y: 1000, weight: 1);
        var vun = new VUN(
            toMove: new FixedEntitySet(entity1, entity2),
            howFar: new FixedNumber(200),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext();

        vun.Execute(context);

        entity1.X.Should().Be(1200);
        entity1.Y.Should().Be(0);
        entity2.X.Should().Be(0);
        entity2.Y.Should().Be(1200);
    }

    [Fact]
    public void Execute_EmitsEntityPushedEvent_PerEntity()
    {
        var entity1 = TestFixtures.MakeEntity(x: 1000, y: 0, weight: 1);
        var entity2 = TestFixtures.MakeEntity(x: 0, y: 1000, weight: 1);
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
        var entity = TestFixtures.MakeEntity(x: 1000, y: 0, weight: 1000);
        var originalX = entity.X;

        var casterEntity = TestFixtures.MakeEntity();
        casterEntity.Reservoir = amount => new ReservoirDraw(0, false);
        var caster = new EntitySet([casterEntity]);

        var result = new SpellResult();
        var vun = new VUN(
            toMove: new FixedEntitySet(entity),
            howFar: new FixedNumber(1000),
            origin: new FixedLocation(0, 0)
        );
        var context = TestFixtures.MakeContext(caster: caster, result: result);

        vun.Execute(context);

        entity.X.Should().Be(originalX);
        result.Events.OfType<EffectNotFiredEvent>().Should().ContainSingle()
            .Which.Effect.Should().Be("VUN");
    }

    [Fact]
    public void Execute_EmptySet_DrawsNoPower()
    {
        var drawn = new List<long>();
        var casterEntity = TestFixtures.MakeEntity();
        casterEntity.Reservoir = amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); };
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
