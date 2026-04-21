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
        var newExecutorEntity = TestFixtures.MakeEntity();
        var newExecutor = new EntitySet([newExecutorEntity]);

        var forked = original.ForkWithNewExecutor(newExecutor);

        forked.Executor.Should().BeSameAs(newExecutor);
    }

    [Fact]
    public void ForkWithNewExecutor_CasterIsPreserved()
    {
        var casterEntity = TestFixtures.MakeEntity();
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
        var pushedEntity = TestFixtures.MakeEntity();
        var pushed = false;
        pushedEntity.Reservoir = amount => { pushed = true; return new ReservoirDraw(amount, false); };
        var pushedSet = new EntitySet([pushedEntity]);

        var original = TestFixtures.MakeContext();
        original.PushPowerSource(pushedSet);

        var forked = original.ForkWithNewExecutor(new EntitySet([]));
        forked.DrawPower(10);

        pushed.Should().BeTrue();
    }
}
