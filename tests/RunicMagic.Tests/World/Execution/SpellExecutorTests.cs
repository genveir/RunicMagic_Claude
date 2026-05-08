using RunicMagic.Controller.Services;
using RunicMagic.World.Entities.Capabilities;
using RunicMagic.World.Execution;
using RunicMagic.World.Runes.EffectRunes;
using RunicMagic.World.Runes.EntityReferenceRunes;
using RunicMagic.World.Runes.EntitySetRunes;
using RunicMagic.World.Runes.ExecutionRunes;
using RunicMagic.World.Runes.LocationRunes;
using RunicMagic.World.Runes.NumberRunes;

namespace RunicMagic.Tests.World.Execution;

public class SpellExecutorTests
{
    [Fact]
    public void Execute_MilestoneSpell_PushesEntitiesInScope()
    {
        var world = new WorldModelBuilder().Build();

        var casterEntity = new EntityBuilder()
            .WithLocation(x: 0, y: 0)
            .WithWeight(1)
            .WithReservoir(draw: amount => new ReservoirDraw(amount, false))
            .Build();
        var target = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(1).Build();
        casterEntity.Scope = () => [target];
        world.Add(casterEntity);
        world.Add(target);
        var ticker = WorldTickerBuilder.ForWorldModel(world).Build();

        var caster = new EntitySet([casterEntity]);
        var executor = new EntitySet([casterEntity]);

        // ZU VUN LA OH IR HOT IR HOT HOT [PAR OH default] — 11 runes
        var spell = new ZU(
            new VUN(
                toMove: new LA(new OH()),
                howFar: new IR(new HOT(), new IR(new HOT(), new HOT())),
                origin: new PAR(new A())
            )
        );

        var spellExecutor = new SpellExecutor(world);
        spellExecutor.Execute(spell, new EventTracker(), 11, caster, executor);

        EventTracker finalTickResult = new EventTracker();
        for (var i = 0; i < 56; i++)
        {
            finalTickResult = new EventTracker();
            ticker.HandleTick(finalTickResult, i);
        }

        target.Location.X.Should().BeApproximately(3744, 0.001);
        target.Location.Y.Should().BeApproximately(0, 0.001);
        finalTickResult.WorldEvents.OfType<EntityPushedEvent>().Should().ContainSingle()
            .Which.Entity.Should().Be(target);
    }

    [Fact]
    public void Execute_DrawsEvaluationCostFromCasterBeforeExecution()
    {
        var drawn = new List<long>();
        var world = new WorldModelBuilder().Build();

        var casterEntity = new EntityBuilder()
            .WithReservoir(draw: amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); })
            .Build();
        world.Add(casterEntity);

        var caster = new EntitySet([casterEntity]);
        var executor = new EntitySet([casterEntity]);

        // Empty set — no execution cost, so only evaluation cost is drawn
        var spell = new ZU(
            new VUN(
                toMove: new FixedEntitySet(),
                howFar: new HET(),
                origin: new FixedLocation(0, 0)
            )
        );

        var spellExecutor = new SpellExecutor(world);
        spellExecutor.Execute(spell, new EventTracker(), 10, caster, executor);

        // evaluation cost = 10 runes × 1_000_000 = 10_000_000
        drawn.Should().ContainSingle().Which.Should().Be(10_000_000);
    }

    [Fact]
    public void Execute_EvaluationCost_DrawnFromExecutorFirst_ThenCaster()
    {
        var executorDrawn = new List<long>();
        var casterDrawn = new List<long>();
        var world = new WorldModelBuilder().Build();

        var executorEntity = new EntityBuilder()
            .WithReservoir(draw: amount => { executorDrawn.Add(amount); return new ReservoirDraw(amount / 2, false); })
            .Build();
        world.Add(executorEntity);

        var casterEntity = new EntityBuilder()
            .WithReservoir(draw: amount => { casterDrawn.Add(amount); return new ReservoirDraw(amount, false); })
            .Build();
        world.Add(casterEntity);

        var caster = new EntitySet([casterEntity]);
        var executor = new EntitySet([executorEntity]);

        var spell = new ZU(new VUN(toMove: new FixedEntitySet(), howFar: new HET(), origin: new FixedLocation(0, 0)));

        var spellExecutor = new SpellExecutor(world);
        spellExecutor.Execute(spell, new EventTracker(), 10, caster, executor);

        // evaluation cost = 10_000_000; executor provides 5_000_000 (amount/2), caster covers remaining 5_000_000
        executorDrawn.Should().ContainSingle().Which.Should().Be(10_000_000);
        casterDrawn.Should().ContainSingle().Which.Should().Be(5_000_000);
    }

    [Fact]
    public void Execute_EvaluationCostCannotBeMet_ExecutorDisintegratesAndSpellAborts()
    {
        var world = new WorldModelBuilder().Build();

        var executorEntity = new EntityBuilder().WithLocation(x: 0, y: 0).Build();
        world.Add(executorEntity);

        // Reservoir that can't pay anything
        var casterEntity = new EntityBuilder()
            .WithReservoir(draw: amount => new ReservoirDraw(0, false))
            .Build();
        world.Add(casterEntity);

        // A target that should NOT be pushed if spell aborts
        var target = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(1).Build();
        executorEntity.Scope = () => [target];
        world.Add(target);

        var caster = new EntitySet([casterEntity]);
        var executor = new EntitySet([executorEntity]);

        var spell = new ZU(
            new VUN(
                toMove: new LA(new OH()),
                howFar: new HET(),
                origin: new PAR(new A())
            )
        );

        var spellExecutor = new SpellExecutor(world);
        var tracker = new EventTracker();
        spellExecutor.Execute(spell, tracker, 10, caster, executor);

        // Executor entity removed from world
        world.Find(executorEntity.Id).Should().BeNull();
        // Target not moved
        target.Location.X.Should().Be(1000);
        // Event emitted
        tracker.WorldEvents.OfType<EntityDisintegratedEvent>().Should().ContainSingle();
    }
}
