using FluentAssertions;
using RunicMagic.Tests.Builders;
using RunicMagic.World;
using RunicMagic.World.Capabilities;
using RunicMagic.World.Execution;
using Xunit;

namespace RunicMagic.Tests.Execution;

public class SpellContextTests
{
    [Fact]
    public void DrawPower_DrainsScopeOfExecutorFirst()
    {
        var scopeEntity = new EntityBuilder()
            .WithReservoir(draw: amount => new ReservoirDraw(amount, false))
            .Build();

        var executorEntity = new EntityBuilder().Build();
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
        var executorEntity = new EntityBuilder()
            .WithReservoir(draw: amount => new ReservoirDraw(amount, false))
            .Build();
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
        var executorEntity = new EntityBuilder()
            .WithReservoir(draw: amount => new ReservoirDraw(0, false))
            .Build();
        var executor = new EntitySet([executorEntity]);

        var scopeOfCasterEntity = new EntityBuilder()
            .WithReservoir(draw: amount => new ReservoirDraw(amount, false))
            .Build();

        var casterEntity = new EntityBuilder().Build();
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
        var executorEntity = new EntityBuilder()
            .WithReservoir(draw: amount => new ReservoirDraw(0, false))
            .Build();
        var executor = new EntitySet([executorEntity]);

        var casterEntity = new EntityBuilder()
            .WithReservoir(draw: amount => new ReservoirDraw(amount, false))
            .Build();
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

        Entity scopeOfExecutorEntity = null!;
        scopeOfExecutorEntity = new EntityBuilder()
            .WithReservoir(draw: amount => { drainOrder.Add(scopeOfExecutorEntity); return new ReservoirDraw(0, false); })
            .Build();

        Entity executorEntity = null!;
        executorEntity = new EntityBuilder()
            .WithReservoir(draw: amount => { drainOrder.Add(executorEntity); return new ReservoirDraw(0, false); })
            .Build();
        executorEntity.Scope = () => [scopeOfExecutorEntity];
        var executor = new EntitySet([executorEntity]);

        Entity scopeOfCasterEntity = null!;
        scopeOfCasterEntity = new EntityBuilder()
            .WithReservoir(draw: amount => { drainOrder.Add(scopeOfCasterEntity); return new ReservoirDraw(0, false); })
            .Build();

        Entity casterEntity = null!;
        casterEntity = new EntityBuilder()
            .WithReservoir(draw: amount => { drainOrder.Add(casterEntity); return new ReservoirDraw(amount, false); })
            .Build();
        casterEntity.Scope = () => [scopeOfCasterEntity];
        var caster = new EntitySet([casterEntity]);

        var context = TestFixtures.MakeContext(caster: caster, executor: executor);
        context.DrawPower(10);

        drainOrder.Should().Equal([scopeOfExecutorEntity, executorEntity, scopeOfCasterEntity, casterEntity]);
    }

    [Fact]
    public void DrawPower_StopsEarlyWhenSatisfied()
    {
        var executorEntity = new EntityBuilder()
            .WithReservoir(draw: amount => new ReservoirDraw(amount, false))
            .Build();
        var executor = new EntitySet([executorEntity]);

        var casterCalled = false;
        var casterEntity = new EntityBuilder()
            .WithReservoir(draw: amount => { casterCalled = true; return new ReservoirDraw(amount, false); })
            .Build();
        var caster = new EntitySet([casterEntity]);

        var context = TestFixtures.MakeContext(caster: caster, executor: executor);
        context.DrawPower(10);

        casterCalled.Should().BeFalse();
    }

    // ── Nested resolution windows ─────────────────────────────────────────────

    [Fact]
    public void NestedWindows_InnerCloseDoesNotDestroyOuterWindow()
    {
        // HORO with default PAR(OH): HOROParser injects EntitySetSelectionCostResolver(OH)
        // as the PAR argument. Resolving it opens and closes an inner window while the outer
        // window (opened by the outer EntitySetSelectionCostResolver, as VUN would) is active.
        // The outer window must survive the inner close so HORO's results are tracked.
        var context = TestFixtures.MakeContext();

        context.OpenResolutionWindow();
        var outerWindow = context.EntityResolutionCount;

        // Inner window: simulates PAR(EntitySetSelectionCostResolver(OH)) resolving
        context.OpenResolutionWindow();
        context.CloseResolutionWindow();

        // Outer window must still be the active window
        context.EntityResolutionCount.Should().NotBeNull();
        context.EntityResolutionCount.Should().BeSameAs(outerWindow);

        // Entities added after the inner close (as HORO would) go into the outer window
        var entityId = EntityId.New();
        context.EntityResolutionCount!.Add(entityId);
        context.EntityResolutionCount.Should().Contain(entityId);

        context.CloseResolutionWindow();
        context.EntityResolutionCount.Should().BeNull();
    }

    // ── ForkWithNewExecutor ───────────────────────────────────────────────────

    [Fact]
    public void ForkWithNewExecutor_ForkedContextHasNewExecutor()
    {
        var original = TestFixtures.MakeContext();
        var newExecutorEntity = new EntityBuilder().Build();
        var newExecutor = new EntitySet([newExecutorEntity]);

        var forked = original.ForkWithNewExecutor(newExecutor);

        forked.Executor.Should().BeSameAs(newExecutor);
    }

    [Fact]
    public void ForkWithNewExecutor_CasterIsPreserved()
    {
        var casterEntity = new EntityBuilder().Build();
        var caster = new EntitySet([casterEntity]);
        var original = TestFixtures.MakeContext(caster: caster);

        var forked = original.ForkWithNewExecutor(new EntitySet([]));

        forked.Caster.Should().BeSameAs(caster);
    }

    [Fact]
    public void ForkWithNewExecutor_WorldIsShared()
    {
        var world = new WorldModel();
        var original = TestFixtures.MakeContext(world: world);

        var forked = original.ForkWithNewExecutor(new EntitySet([]));

        forked.World.Should().BeSameAs(world);
    }

    [Fact]
    public void ForkWithNewExecutor_ResultIsShared()
    {
        var result = new SpellResult();
        var original = TestFixtures.MakeContext(result: result);

        var forked = original.ForkWithNewExecutor(new EntitySet([]));

        forked.Result.Should().BeSameAs(result);
    }

    [Fact]
    public void ForkWithNewExecutor_EntityResolutionCountNull_ForkedIsAlsoNull()
    {
        var original = TestFixtures.MakeContext();

        var forked = original.ForkWithNewExecutor(new EntitySet([]));

        forked.EntityResolutionCount.Should().BeNull();
    }

    [Fact]
    public void ForkWithNewExecutor_EntityResolutionCountOpen_ForkedReceivesCopy()
    {
        var original = TestFixtures.MakeContext();
        original.OpenResolutionWindow();
        var entityId = EntityId.New();
        original.EntityResolutionCount!.Add(entityId);

        var forked = original.ForkWithNewExecutor(new EntitySet([]));

        forked.EntityResolutionCount.Should().NotBeNull();
        forked.EntityResolutionCount.Should().Contain(entityId);
        forked.EntityResolutionCount.Should().NotBeSameAs(original.EntityResolutionCount);
    }

    [Fact]
    public void ForkWithNewExecutor_PowerSourceStackIsCopied()
    {
        var pushed = false;
        var pushedEntity = new EntityBuilder()
            .WithReservoir(draw: amount => { pushed = true; return new ReservoirDraw(amount, false); })
            .Build();
        var pushedSet = new EntitySet([pushedEntity]);

        var original = TestFixtures.MakeContext();
        original.PushPowerSource(pushedSet);

        var forked = original.ForkWithNewExecutor(new EntitySet([]));
        forked.DrawPower(10);

        pushed.Should().BeTrue();
    }
}
