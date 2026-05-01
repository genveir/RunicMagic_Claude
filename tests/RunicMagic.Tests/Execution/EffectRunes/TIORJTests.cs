using RunicMagic.Controller.Services;
using RunicMagic.Tests.Builders;
using RunicMagic.World.Capabilities;
using RunicMagic.World.Execution;
using RunicMagic.World.Runes.EffectRunes;

namespace RunicMagic.Tests.Execution.EffectRunes;

public class TIORJTests
{
    [Fact]
    public void Execute_DrawsFromSourceAndFillsTarget()
    {
        var drawn = new List<long>();
        var source = new EntityBuilder()
            .WithReservoir(draw: amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); })
            .Build();

        var filled = new List<long>();
        var target = new EntityBuilder()
            .WithReservoir(fill: amount => { filled.Add(amount); return new ReservoirFill(amount, false); })
            .Build();

        var tiorj = new TIORJ(
            from: new FixedEntitySet(source),
            to: new FixedEntitySet(target),
            amount: new FixedNumber(50)
        );

        tiorj.Execute(TestFixtures.MakeContext());

        drawn.Should().ContainSingle().Which.Should().Be(50);
        filled.Should().ContainSingle().Which.Should().Be(50);
    }

    [Fact]
    public void Execute_SourceCannotProvideFullAmount_FillsWhatWasDrawn()
    {
        var source = new EntityBuilder()
            .WithReservoir(draw: amount => new ReservoirDraw(amount / 2, false))
            .Build();

        var filled = new List<long>();
        var target = new EntityBuilder()
            .WithReservoir(fill: amount => { filled.Add(amount); return new ReservoirFill(amount, false); })
            .Build();

        var tiorj = new TIORJ(
            from: new FixedEntitySet(source),
            to: new FixedEntitySet(target),
            amount: new FixedNumber(100)
        );

        tiorj.Execute(TestFixtures.MakeContext());

        filled.Should().ContainSingle().Which.Should().Be(50);
    }

    [Fact]
    public void Execute_TargetOverflows_DamagesTarget()
    {
        var source = new EntityBuilder()
            .WithReservoir(draw: amount => new ReservoirDraw(amount, false))
            .Build();

        var target = new EntityBuilder()
            .WithStructuralIntegrity(max: 100, current: 100)
            .WithReservoir(fill: _ => new ReservoirFill(0, true))
            .Build();

        var tiorj = new TIORJ(
            from: new FixedEntitySet(source),
            to: new FixedEntitySet(target),
            amount: new FixedNumber(5)
        );

        tiorj.Execute(TestFixtures.MakeContext());

        target.StructuralIntegrity.CurrentIntegrity.Should().Be(90);
    }

    [Fact]
    public void Execute_OverflowReturnedToSource()
    {
        var sourceFilled = new List<long>();
        var source = new EntityBuilder()
            .WithReservoir(
                draw: amount => new ReservoirDraw(amount, false),
                fill: amount => { sourceFilled.Add(amount); return new ReservoirFill(amount, false); })
            .Build();

        var world = new global::RunicMagic.World.WorldModel();
        // target has 1 hp: absorbs nothing, 1 damage, ceil(1/2)=1 consumed, 9 returned to source
        var target = new EntityBuilder()
            .WithStructuralIntegrity(max: 1, current: 1)
            .WithReservoir(fill: _ => new ReservoirFill(0, true))
            .Build();
        world.Add(target);

        var tiorj = new TIORJ(
            from: new FixedEntitySet(source),
            to: new FixedEntitySet(target),
            amount: new FixedNumber(10)
        );

        tiorj.Execute(TestFixtures.MakeContext(world: world));

        sourceFilled.Should().ContainSingle().Which.Should().Be(9);
    }

    [Fact]
    public void Execute_EmitsEventsForDrawAndFill()
    {
        var source = new EntityBuilder()
            .WithReservoir(draw: amount => new ReservoirDraw(amount, false))
            .Build();
        var target = new EntityBuilder()
            .WithReservoir(fill: amount => new ReservoirFill(amount, false))
            .Build();

        var result = new EventTracker();
        var tiorj = new TIORJ(
            from: new FixedEntitySet(source),
            to: new FixedEntitySet(target),
            amount: new FixedNumber(20)
        );

        tiorj.Execute(TestFixtures.MakeContext(result: result));

        result.WorldEvents.OfType<PowerDrawnEvent>().Should().ContainSingle().Which.Entity.Should().Be(source);
        result.WorldEvents.OfType<PowerFilledEvent>().Should().ContainSingle().Which.Entity.Should().Be(target);
    }
}
