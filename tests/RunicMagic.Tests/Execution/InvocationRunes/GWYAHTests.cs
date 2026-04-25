using FluentAssertions;
using RunicMagic.Tests.Builders;
using RunicMagic.World;
using RunicMagic.World.Capabilities;
using RunicMagic.World.Execution;
using RunicMagic.World.Runes.InvocationRunes;
using RunicMagic.World.Runes.RuneTypes;
using Xunit;

namespace RunicMagic.Tests.Execution.InvocationRunes;

public class GWYAHTests
{
    private static SpellContext ContextWithCaster(Entity casterEntity)
    {
        var caster = new EntitySet([casterEntity]);
        return TestFixtures.MakeContext(caster: caster, executor: caster);
    }

    // ── No inscriptions ───────────────────────────────────────────────────────

    [Fact]
    public void Execute_EntityWithNoInscriptions_FiresNothing()
    {
        var target = new EntityBuilder().Build();
        var context = TestFixtures.MakeContext();
        var gwyah = new GWYAH(target: new FixedEntitySet(target));

        gwyah.Execute(context);

        context.Result.Events.Should().BeEmpty();
    }

    // ── Executor is swapped ───────────────────────────────────────────────────

    [Fact]
    public void Execute_FiresInscriptionWithInscribedEntityAsExecutor()
    {
        Entity? capturedExecutor = null;
        var inscription = new CaptureExecutorStatement(e => capturedExecutor = e);

        var target = new EntityBuilder().Build();
        target.ParsedInscriptions = [inscription];

        var context = TestFixtures.MakeContext();
        var gwyah = new GWYAH(target: new FixedEntitySet(target));

        gwyah.Execute(context);

        capturedExecutor.Should().BeSameAs(target);
    }

    [Fact]
    public void Execute_CasterIsPreservedInFork()
    {
        EntitySet? capturedCaster = null;
        var inscription = new CaptureCasterStatement(c => capturedCaster = c);

        var casterEntity = new EntityBuilder().Build();
        var caster = new EntitySet([casterEntity]);

        var target = new EntityBuilder().Build();
        target.ParsedInscriptions = [inscription];

        var context = TestFixtures.MakeContext(caster: caster, executor: caster);
        var gwyah = new GWYAH(target: new FixedEntitySet(target));

        gwyah.Execute(context);

        capturedCaster.Should().BeSameAs(caster);
    }

    // ── Multiple inscriptions ─────────────────────────────────────────────────

    [Fact]
    public void Execute_MultipleInscriptionsOnOneEntity_AllFiredInOrder()
    {
        var firedOrder = new List<int>();
        var target = new EntityBuilder().Build();
        target.ParsedInscriptions =
        [
            new RecordOrderStatement(firedOrder, 1),
            new RecordOrderStatement(firedOrder, 2),
        ];

        var context = TestFixtures.MakeContext();
        var gwyah = new GWYAH(target: new FixedEntitySet(target));

        gwyah.Execute(context);

        firedOrder.Should().Equal([1, 2]);
    }

    [Fact]
    public void Execute_MultipleEntities_InscriptionsFiredEntityFirst()
    {
        var firedOrder = new List<string>();

        var first = new EntityBuilder().Build();
        first.ParsedInscriptions =
        [
            new RecordLabelStatement(firedOrder, "first-a"),
            new RecordLabelStatement(firedOrder, "first-b"),
        ];

        var second = new EntityBuilder().Build();
        second.ParsedInscriptions =
        [
            new RecordLabelStatement(firedOrder, "second-a"),
        ];

        var context = TestFixtures.MakeContext();
        var gwyah = new GWYAH(target: new FixedEntitySet(first, second));

        gwyah.Execute(context);

        firedOrder.Should().Equal(["first-a", "first-b", "second-a"]);
    }

    // ── Power stack travels ───────────────────────────────────────────────────

    [Fact]
    public void Execute_PowerSourceStackTravelsIntoFork()
    {
        var pushed = false;
        var pushedEntity = new EntityBuilder()
            .WithReservoir(draw: amount => { pushed = true; return new ReservoirDraw(amount, false); })
            .Build();
        var pushedSet = new EntitySet([pushedEntity]);

        long? drawnInsideInscription = null;
        var inscription = new DrawPowerStatement(10, drawn => drawnInsideInscription = drawn);

        var target = new EntityBuilder().Build();
        target.ParsedInscriptions = [inscription];

        var context = TestFixtures.MakeContext();
        context.PushPowerSource(pushedSet);

        var gwyah = new GWYAH(target: new FixedEntitySet(target));
        gwyah.Execute(context);

        pushed.Should().BeTrue();
    }

    // ── Test doubles ──────────────────────────────────────────────────────────

    private class CaptureExecutorStatement(Action<Entity> capture) : IStatement
    {
        public void Execute(SpellContext context)
        {
            capture(context.Executor.Entities.Single());
        }
    }

    private class CaptureCasterStatement(Action<EntitySet> capture) : IStatement
    {
        public void Execute(SpellContext context)
        {
            capture(context.Caster);
        }
    }

    private class RecordOrderStatement(List<int> log, int index) : IStatement
    {
        public void Execute(SpellContext context)
        {
            log.Add(index);
        }
    }

    private class RecordLabelStatement(List<string> log, string label) : IStatement
    {
        public void Execute(SpellContext context)
        {
            log.Add(label);
        }
    }

    private class DrawPowerStatement(long amount, Action<long> onDrawn) : IStatement
    {
        public void Execute(SpellContext context)
        {
            var drawn = context.DrawPower(amount);
            onDrawn(drawn);
        }
    }
}
