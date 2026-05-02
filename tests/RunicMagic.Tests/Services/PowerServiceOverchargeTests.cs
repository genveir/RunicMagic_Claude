using RunicMagic.Controller.Services;
using RunicMagic.World;
using RunicMagic.World.Capabilities;
using RunicMagic.World.Execution;
using RunicMagic.World.Services;

namespace RunicMagic.Tests.Services;

public class PowerServiceOverchargeTests
{
    [Fact]
    public void FillWithOvercharge_NoOverflow_FillsTargetAndStops()
    {
        var filled = new List<long>();
        var target = new EntityBuilder()
            .WithReservoir(fill: amount => { filled.Add(amount); return new ReservoirFill(amount, false); })
            .Build();
        var targetSet = new EntitySet([target]);

        PowerService.FillWithOvercharge(targetSet, null, 10, TestFixtures.MakeContext());

        filled.Should().ContainSingle().Which.Should().Be(10);
    }

    [Fact]
    public void FillWithOvercharge_Overflow_DamagesTarget()
    {
        var target = new EntityBuilder()
            .WithStructuralIntegrity(max: 100, current: 100)
            .WithReservoir(fill: _ => new ReservoirFill(0, true))
            .Build();
        var targetSet = new EntitySet([target]);

        // 5 power remaining after fill (fill absorbs nothing) → 10 damage
        PowerService.FillWithOvercharge(targetSet, null, 5, TestFixtures.MakeContext());

        target.StructuralIntegrity.CurrentIntegrity.Should().Be(90);
    }

    [Fact]
    public void FillWithOvercharge_Overflow_PowerConsumedIsCeilingOfHalfDamageDealt()
    {
        // Target has 1 hp so it can only absorb 1 damage even though 2 are owed per power.
        // 1 damage dealt → ceil(1/2) = 1 power consumed. With 1 power in, nothing should cascade.
        var world = new WorldModel();
        var target = new EntityBuilder()
            .WithStructuralIntegrity(max: 1, current: 1)
            .WithReservoir(fill: _ => new ReservoirFill(0, true))
            .Build();
        world.Add(target);
        var targetSet = new EntitySet([target]);

        var result = new EventTracker();
        var context = TestFixtures.MakeContext(world: world, result: result);

        PowerService.FillWithOvercharge(targetSet, null, 1, context);

        result.WorldEvents.OfType<EntityDisintegratedEvent>().Should().ContainSingle().Which.Entity.Should().Be(target);
        result.WorldEvents.OfType<EntityDamagedEvent>().Should().BeEmpty("entity disintegrated, not merely damaged");
    }

    [Fact]
    public void FillWithOvercharge_Overflow_CascadesToScope()
    {
        var scopeEntity = new EntityBuilder()
            .WithStructuralIntegrity(max: 100, current: 100)
            .WithReservoir(fill: amount => new ReservoirFill(amount, false))
            .Build();

        var target = new EntityBuilder()
            .WithStructuralIntegrity(max: 100, current: 100)
            .WithReservoir(fill: _ => new ReservoirFill(0, true))
            .WithScope(() => [scopeEntity])
            .Build();
        var targetSet = new EntitySet([target]);

        var result = new EventTracker();
        var context = TestFixtures.MakeContext(result: result);

        // 6 power, target absorbs nothing → 12 damage to target → 6 power consumed
        // remaining = 0, scope is never reached
        PowerService.FillWithOvercharge(targetSet, null, 6, context);

        result.WorldEvents.OfType<PowerFilledEvent>()
            .Where(e => e.Entity == scopeEntity)
            .Should().BeEmpty("all power was consumed damaging the target");
    }

    [Fact]
    public void FillWithOvercharge_OverflowAfterPartialFill_CascadesToScope()
    {
        var scopeFilled = new List<long>();
        var scopeEntity = new EntityBuilder()
            .WithReservoir(fill: amount => { scopeFilled.Add(amount); return new ReservoirFill(amount, false); })
            .Build();

        // target absorbs 2 of 10, remaining 8 → 16 damage to target (100 hp) → 8 power consumed → 0 remaining
        // but let's make target absorb 8 of 10, leaving 2 remaining
        var target = new EntityBuilder()
            .WithStructuralIntegrity(max: 100, current: 100)
            .WithReservoir(fill: amount => new ReservoirFill(Math.Min(amount, 8), amount <= 8))
            .WithScope(() => [scopeEntity])
            .Build();
        var targetSet = new EntitySet([target]);

        PowerService.FillWithOvercharge(targetSet, null, 10, TestFixtures.MakeContext());

        // 2 power remaining after partial fill → 4 damage to target → ceil(4/2)=2 power consumed → 0 remaining
        // scope fill should not be reached
        scopeFilled.Should().BeEmpty("all remaining power consumed damaging target");
    }

    [Fact]
    public void FillWithOvercharge_Overflow_ScopeFilledBeforeDamageConsumedAll()
    {
        var scopeFilled = new List<long>();
        var scopeEntity = new EntityBuilder()
            .WithReservoir(fill: amount => { scopeFilled.Add(amount); return new ReservoirFill(amount, false); })
            .Build();

        // target absorbs nothing, has 1 hp → 2 damage dealt from 10 remaining → ceil(2/2)=1 consumed → 9 remaining
        // wait, target has 1 hp: damage = min(2*10, 1) = 1 → ceil(1/2)=1 consumed → 9 remaining
        // then scope gets 9 power
        var world = new WorldModel();
        var target = new EntityBuilder()
            .WithStructuralIntegrity(max: 1, current: 1)
            .WithReservoir(fill: _ => new ReservoirFill(0, true))
            .WithScope(() => [scopeEntity])
            .Build();
        world.Add(target);
        var targetSet = new EntitySet([target]);

        PowerService.FillWithOvercharge(targetSet, null, 10, TestFixtures.MakeContext(world: world));

        scopeFilled.Should().ContainSingle().Which.Should().Be(9);
    }

    [Fact]
    public void FillWithOvercharge_ExhaustedScope_DamagesExecutor()
    {
        var world = new WorldModel();
        var executorEntity = new EntityBuilder()
            .WithStructuralIntegrity(max: 100, current: 100)
            .Build();
        var executor = new EntitySet([executorEntity]);

        // target: no reservoir capacity, 1 hp → disintegrates, 1 power consumed, 9 remaining
        // scope: empty (no entities with reservoirs)
        // executor takes 18 damage
        var target = new EntityBuilder()
            .WithStructuralIntegrity(max: 1, current: 1)
            .WithReservoir(fill: _ => new ReservoirFill(0, true))
            .Build();
        world.Add(target);
        var targetSet = new EntitySet([target]);

        PowerService.FillWithOvercharge(targetSet, null, 10, TestFixtures.MakeContext(executor: executor, world: world));

        executorEntity.StructuralIntegrity.CurrentIntegrity.Should().Be(82);
    }

    [Fact]
    public void FillWithOvercharge_ExecutorAndScopeExhausted_DamagesCaster()
    {
        var world = new WorldModel();
        var casterEntity = new EntityBuilder()
            .WithStructuralIntegrity(max: 100, current: 100)
            .Build();
        var caster = new EntitySet([casterEntity]);

        var target = new EntityBuilder()
            .WithStructuralIntegrity(max: 1, current: 1)
            .WithReservoir(fill: _ => new ReservoirFill(0, true))
            .Build();
        world.Add(target);
        var targetSet = new EntitySet([target]);

        // target: 1 hp → 1 damage, ceil(1/2)=1 consumed, 9 remaining
        // executor: empty (no entities) → 0 damage, 9 remaining
        // caster: takes 18 damage
        PowerService.FillWithOvercharge(targetSet, null, 10, TestFixtures.MakeContext(caster: caster, world: world));

        casterEntity.StructuralIntegrity.CurrentIntegrity.Should().Be(82);
    }

    [Fact]
    public void FillWithOvercharge_RemainingAfterCaster_ReturnedToSource()
    {
        var sourceFilled = new List<long>();
        var sourceEntity = new EntityBuilder()
            .WithReservoir(fill: amount => { sourceFilled.Add(amount); return new ReservoirFill(amount, false); })
            .Build();
        var sourceSet = new EntitySet([sourceEntity]);

        var target = new EntityBuilder()
            .WithStructuralIntegrity(max: 1, current: 1)
            .WithReservoir(fill: _ => new ReservoirFill(0, true))
            .Build();
        var world = new WorldModel();
        world.Add(target);
        var targetSet = new EntitySet([target]);

        // target: 1 hp → 1 damage, 1 power consumed, 9 remaining
        // executor & caster: empty → no damage
        // source absorbs 9
        PowerService.FillWithOvercharge(targetSet, sourceSet, 10, TestFixtures.MakeContext(world: world));

        sourceFilled.Should().ContainSingle().Which.Should().Be(9);
    }

    [Fact]
    public void FillWithOvercharge_NullSource_RemainingDissipates()
    {
        var world = new WorldModel();
        var target = new EntityBuilder()
            .WithStructuralIntegrity(max: 1, current: 1)
            .WithReservoir(fill: _ => new ReservoirFill(0, true))
            .Build();
        world.Add(target);
        var targetSet = new EntitySet([target]);

        var result = new EventTracker();
        var act = () => PowerService.FillWithOvercharge(targetSet, null, 10, TestFixtures.MakeContext(world: world, result: result));

        act.Should().NotThrow();
    }

    [Fact]
    public void FillWithOvercharge_ScopeFilteredToReservoirEntitiesOnly()
    {
        var noReservoirFilled = false;
        var withReservoirFilled = new List<long>();

        var scopeNoReservoir = new EntityBuilder().Build();
        var scopeWithReservoir = new EntityBuilder()
            .WithReservoir(fill: amount => { withReservoirFilled.Add(amount); return new ReservoirFill(amount, false); })
            .Build();

        var world = new WorldModel();
        var target = new EntityBuilder()
            .WithStructuralIntegrity(max: 1, current: 1)
            .WithReservoir(fill: _ => new ReservoirFill(0, true))
            .WithScope(() => [scopeNoReservoir, scopeWithReservoir])
            .Build();
        world.Add(target);
        var targetSet = new EntitySet([target]);

        PowerService.FillWithOvercharge(targetSet, null, 10, TestFixtures.MakeContext(world: world));

        noReservoirFilled.Should().BeFalse();
        withReservoirFilled.Should().NotBeEmpty();
    }
}
