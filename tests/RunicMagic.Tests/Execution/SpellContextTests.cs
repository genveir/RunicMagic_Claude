using FluentAssertions;
using RunicMagic.World;
using RunicMagic.World.Execution;
using Xunit;

namespace RunicMagic.Tests.Execution;

public class SpellContextTests
{
    [Fact]
    public void DrawPower_DrainsScopeOfExecutorFirst()
    {
        var scopeEntity = TestFixtures.MakeEntity();
        scopeEntity.Reservoir = amount => new ReservoirDraw(amount, false);

        var executorEntity = TestFixtures.MakeEntity();
        executorEntity.Scope = () => [scopeEntity];
        var executor = new EntitySet([executorEntity]);

        var context = TestFixtures.MakeContext(executor: executor);
        var drawn = context.DrawPower(10);

        drawn.Should().Be(10);
        var events = context.Result.Events.OfType<PowerDrawnEvent>().ToList();
        events.Should().ContainSingle().Which.Entity.Should().Be(scopeEntity);
    }

    [Fact]
    public void DrawPower_FallsToExecutorWhenScopeEmpty()
    {
        var executorEntity = TestFixtures.MakeEntity();
        executorEntity.Reservoir = amount => new ReservoirDraw(amount, false);
        var executor = new EntitySet([executorEntity]);

        var context = TestFixtures.MakeContext(executor: executor);
        var drawn = context.DrawPower(10);

        drawn.Should().Be(10);
        var events = context.Result.Events.OfType<PowerDrawnEvent>().ToList();
        events.Should().ContainSingle().Which.Entity.Should().Be(executorEntity);
    }

    [Fact]
    public void DrawPower_FallsToScopeOfCasterWhenExecutorExhausted()
    {
        var executorEntity = TestFixtures.MakeEntity();
        executorEntity.Reservoir = amount => new ReservoirDraw(0, false);
        var executor = new EntitySet([executorEntity]);

        var scopeOfCasterEntity = TestFixtures.MakeEntity();
        scopeOfCasterEntity.Reservoir = amount => new ReservoirDraw(amount, false);

        var casterEntity = TestFixtures.MakeEntity();
        casterEntity.Scope = () => [scopeOfCasterEntity];
        var caster = new EntitySet([casterEntity]);

        var context = TestFixtures.MakeContext(caster: caster, executor: executor);
        var drawn = context.DrawPower(10);

        drawn.Should().Be(10);
        var events = context.Result.Events.OfType<PowerDrawnEvent>().ToList();
        events.Should().ContainSingle().Which.Entity.Should().Be(scopeOfCasterEntity);
    }

    [Fact]
    public void DrawPower_FallsToCasterLast()
    {
        var executorEntity = TestFixtures.MakeEntity();
        executorEntity.Reservoir = amount => new ReservoirDraw(0, false);
        var executor = new EntitySet([executorEntity]);

        var casterEntity = TestFixtures.MakeEntity();
        casterEntity.Reservoir = amount => new ReservoirDraw(amount, false);
        var caster = new EntitySet([casterEntity]);

        var context = TestFixtures.MakeContext(caster: caster, executor: executor);
        var drawn = context.DrawPower(10);

        drawn.Should().Be(10);
        var events = context.Result.Events.OfType<PowerDrawnEvent>().ToList();
        events.Should().ContainSingle().Which.Entity.Should().Be(casterEntity);
    }

    [Fact]
    public void DrawPower_FullFallbackOrder_DrainsInSequence()
    {
        var drainOrder = new List<Entity>();

        var scopeOfExecutorEntity = TestFixtures.MakeEntity();
        scopeOfExecutorEntity.Reservoir = amount => { drainOrder.Add(scopeOfExecutorEntity); return new ReservoirDraw(0, false); };

        var executorEntity = TestFixtures.MakeEntity();
        executorEntity.Scope = () => [scopeOfExecutorEntity];
        executorEntity.Reservoir = amount => { drainOrder.Add(executorEntity); return new ReservoirDraw(0, false); };
        var executor = new EntitySet([executorEntity]);

        var scopeOfCasterEntity = TestFixtures.MakeEntity();
        scopeOfCasterEntity.Reservoir = amount => { drainOrder.Add(scopeOfCasterEntity); return new ReservoirDraw(0, false); };

        var casterEntity = TestFixtures.MakeEntity();
        casterEntity.Scope = () => [scopeOfCasterEntity];
        casterEntity.Reservoir = amount => { drainOrder.Add(casterEntity); return new ReservoirDraw(amount, false); };
        var caster = new EntitySet([casterEntity]);

        var context = TestFixtures.MakeContext(caster: caster, executor: executor);
        context.DrawPower(10);

        drainOrder.Should().Equal([scopeOfExecutorEntity, executorEntity, scopeOfCasterEntity, casterEntity]);
    }

    [Fact]
    public void DrawPower_StopsEarlyWhenSatisfied()
    {
        var executorEntity = TestFixtures.MakeEntity();
        executorEntity.Reservoir = amount => new ReservoirDraw(amount, false);
        var executor = new EntitySet([executorEntity]);

        var casterEntity = TestFixtures.MakeEntity();
        var casterCalled = false;
        casterEntity.Reservoir = amount => { casterCalled = true; return new ReservoirDraw(amount, false); };
        var caster = new EntitySet([casterEntity]);

        var context = TestFixtures.MakeContext(caster: caster, executor: executor);
        context.DrawPower(10);

        casterCalled.Should().BeFalse();
    }
}
