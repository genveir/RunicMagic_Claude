using FluentAssertions;
using RunicMagic.World;
using RunicMagic.World.Execution;
using RunicMagic.World.Runes.EffectRunes;
using RunicMagic.World.Runes.EntityReferenceRunes;
using RunicMagic.World.Runes.EntitySetRunes;
using RunicMagic.World.Runes.ExecutionRunes;
using RunicMagic.World.Runes.LocationRunes;
using RunicMagic.World.Runes.NumberRunes;
using Xunit;

namespace RunicMagic.Tests.Execution;

public class SpellExecutorTests
{
    [Fact]
    public void Execute_MilestoneSpell_PushesEntitiesInScope()
    {
        var world = new WorldModel();

        var casterEntity = TestFixtures.MakeEntity(x: 0, y: 0, weight: 1);
        casterEntity.Reservoir = amount => new ReservoirDraw(amount, false);
        var target = TestFixtures.MakeEntity(x: 1000, y: 0, weight: 1);
        casterEntity.Scope = () => [target];
        world.Add(casterEntity);
        world.Add(target);

        var caster = new EntitySet([casterEntity]);
        var executor = new EntitySet([casterEntity]);

        // ZU VUN LA OH FOTIR FOTIR FOTIR HET [PAR A default] — 10 runes
        var spell = new ZU(
            new VUN(
                toMove: new LA(new OH()),
                howFar: new FOTIR(new FOTIR(new FOTIR(new HET()))),
                origin: new PAR(new A())
            )
        );

        var spellExecutor = new SpellExecutor(world);
        var result = spellExecutor.Execute(spell, runeCount: 10, caster, executor);

        target.Location.X.Should().Be(3744);
        target.Location.Y.Should().Be(0);
        result.Events.OfType<EntityPushedEvent>().Should().ContainSingle()
            .Which.Entity.Should().Be(target);
    }

    [Fact]
    public void Execute_DrawsEvaluationCostFromCasterBeforeExecution()
    {
        var drawn = new List<long>();
        var world = new WorldModel();

        var casterEntity = TestFixtures.MakeEntity();
        casterEntity.Reservoir = amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); };
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
        spellExecutor.Execute(spell, runeCount: 10, caster, executor);

        // evaluation cost = 10 / 5 = 2
        drawn.Should().ContainSingle().Which.Should().Be(2);
    }

    [Fact]
    public void Execute_EvaluationCost_DrawnFromExecutorFirst_ThenCaster()
    {
        var executorDrawn = new List<long>();
        var casterDrawn = new List<long>();
        var world = new WorldModel();

        var executorEntity = TestFixtures.MakeEntity();
        executorEntity.Reservoir = amount => { executorDrawn.Add(amount); return new ReservoirDraw(amount / 2, false); };
        world.Add(executorEntity);

        var casterEntity = TestFixtures.MakeEntity();
        casterEntity.Reservoir = amount => { casterDrawn.Add(amount); return new ReservoirDraw(amount, false); };
        world.Add(casterEntity);

        var caster = new EntitySet([casterEntity]);
        var executor = new EntitySet([executorEntity]);

        var spell = new ZU(new VUN(toMove: new FixedEntitySet(), howFar: new HET(), origin: new FixedLocation(0, 0)));

        var spellExecutor = new SpellExecutor(world);
        spellExecutor.Execute(spell, runeCount: 10, caster, executor);

        // evaluation cost = 2; executor provides 1, caster covers remaining 1
        executorDrawn.Should().ContainSingle().Which.Should().Be(2);
        casterDrawn.Should().ContainSingle().Which.Should().Be(1);
    }

    [Fact]
    public void Execute_RuneCountBelowFive_DrawsZeroEvaluationCost()
    {
        var drawn = new List<long>();
        var world = new WorldModel();

        var casterEntity = TestFixtures.MakeEntity();
        casterEntity.Reservoir = amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); };
        world.Add(casterEntity);

        var caster = new EntitySet([casterEntity]);
        var executor = new EntitySet([casterEntity]);

        var spell = new ZU(
            new VUN(
                toMove: new FixedEntitySet(),
                howFar: new HET(),
                origin: new FixedLocation(0, 0)
            )
        );

        var spellExecutor = new SpellExecutor(world);
        spellExecutor.Execute(spell, runeCount: 4, caster, executor);

        drawn.Should().BeEmpty();
    }

    [Fact]
    public void Execute_EvaluationCostCannotBeMet_ExecutorDisintegratesAndSpellAborts()
    {
        var world = new WorldModel();

        var executorEntity = TestFixtures.MakeEntity(x: 0, y: 0);
        world.Add(executorEntity);

        var casterEntity = TestFixtures.MakeEntity();
        // Reservoir that can't pay anything
        casterEntity.Reservoir = amount => new ReservoirDraw(0, false);
        world.Add(casterEntity);

        // A target that should NOT be pushed if spell aborts
        var target = TestFixtures.MakeEntity(x: 1000, y: 0, weight: 1);
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
        var result = spellExecutor.Execute(spell, runeCount: 10, caster, executor);

        // Executor entity removed from world
        world.Find(executorEntity.Id).Should().BeNull();
        // Target not moved
        target.Location.X.Should().Be(1000);
        // Event emitted
        result.Events.OfType<ExecutorDisintegratedEvent>().Should().ContainSingle();
    }
}
