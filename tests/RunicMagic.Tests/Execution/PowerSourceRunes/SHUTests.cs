using FluentAssertions;
using RunicMagic.World.Execution;
using RunicMagic.World.Runes.PowerSourceRunes;
using RunicMagic.World.Runes.RuneTypes;
using Xunit;

namespace RunicMagic.Tests.Execution.PowerSourceRunes;

public class SHUTests
{
    [Fact]
    public void Execute_DrawsFromPushedSourceBeforeExecutor()
    {
        var drawOrder = new List<string>();

        var sourceEntity = TestFixtures.MakeEntity();
        sourceEntity.Reservoir = amount => { drawOrder.Add("source"); return new ReservoirDraw(amount, false); };

        var executorEntity = TestFixtures.MakeEntity();
        executorEntity.Reservoir = amount => { drawOrder.Add("executor"); return new ReservoirDraw(amount, false); };
        var executor = new EntitySet([executorEntity]);

        var context = TestFixtures.MakeContext(executor: executor);
        var shu = new SHU(
            source: new FixedEntitySet(sourceEntity),
            statement: new DrawingStatement(amount: 1)
        );

        shu.Execute(context);

        drawOrder[0].Should().Be("source");
    }

    [Fact]
    public void Execute_PopsSourceAfterStatement_SubsequentDrawSkipsSource()
    {
        var sourceDrawn = false;

        var sourceEntity = TestFixtures.MakeEntity();
        sourceEntity.Reservoir = amount => { sourceDrawn = true; return new ReservoirDraw(amount, false); };

        var casterEntity = TestFixtures.MakeEntity();
        casterEntity.Reservoir = amount => new ReservoirDraw(amount, false);
        var caster = new EntitySet([casterEntity]);

        var context = TestFixtures.MakeContext(caster: caster);
        var shu = new SHU(
            source: new FixedEntitySet(sourceEntity),
            statement: new NoOpStatement()
        );

        shu.Execute(context);
        sourceDrawn = false;
        context.DrawPower(1);

        sourceDrawn.Should().BeFalse();
    }

    [Fact]
    public void Execute_Nested_InnerSourceDrawsBeforeOuter()
    {
        var drawOrder = new List<string>();

        var outerEntity = TestFixtures.MakeEntity();
        outerEntity.Reservoir = amount => { drawOrder.Add("outer"); return new ReservoirDraw(0, false); };

        var innerEntity = TestFixtures.MakeEntity();
        innerEntity.Reservoir = amount => { drawOrder.Add("inner"); return new ReservoirDraw(0, false); };

        var casterEntity = TestFixtures.MakeEntity();
        casterEntity.Reservoir = amount => { drawOrder.Add("caster"); return new ReservoirDraw(amount, false); };
        var caster = new EntitySet([casterEntity]);

        var context = TestFixtures.MakeContext(caster: caster);
        var inner = new SHU(
            source: new FixedEntitySet(innerEntity),
            statement: new DrawingStatement(amount: 1)
        );
        var outer = new SHU(
            source: new FixedEntitySet(outerEntity),
            statement: inner
        );

        outer.Execute(context);

        drawOrder.Should().Equal("inner", "outer", "caster");
    }

    [Fact]
    public void Execute_SourceDepleted_FallsBackToExecutorThenCaster()
    {
        var drawOrder = new List<string>();

        var sourceEntity = TestFixtures.MakeEntity();
        sourceEntity.Reservoir = amount => { drawOrder.Add("source"); return new ReservoirDraw(0, false); };

        var executorEntity = TestFixtures.MakeEntity();
        executorEntity.Reservoir = amount => { drawOrder.Add("executor"); return new ReservoirDraw(0, false); };
        var executor = new EntitySet([executorEntity]);

        var casterEntity = TestFixtures.MakeEntity();
        casterEntity.Reservoir = amount => { drawOrder.Add("caster"); return new ReservoirDraw(amount, false); };
        var caster = new EntitySet([casterEntity]);

        var context = TestFixtures.MakeContext(caster: caster, executor: executor);
        var shu = new SHU(
            source: new FixedEntitySet(sourceEntity),
            statement: new DrawingStatement(amount: 1)
        );

        shu.Execute(context);

        drawOrder.Should().Equal("source", "executor", "caster");
    }

    private class DrawingStatement(long amount) : IStatement
    {
        public void Execute(SpellContext context)
        {
            context.DrawPower(amount);
        }
    }

    private class NoOpStatement : IStatement
    {
        public void Execute(SpellContext context) { }
    }
}
