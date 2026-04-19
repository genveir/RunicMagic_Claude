using FluentAssertions;
using RunicMagic.World.Runes.NumberRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.ArithmeticRunes;

public class IRTests
{
    [Fact]
    public void Evaluate_IR_HET_HET_ReturnsOne()
    {
        var ir = new IR(new HET(), new HET());
        var context = TestFixtures.MakeContext();

        var result = ir.Evaluate(context);

        result.Value.Should().Be(1);
    }

    [Fact]
    public void Evaluate_IR_HOT_HET_ReturnsFourteen()
    {
        var ir = new IR(new HOT(), new HET());
        var context = TestFixtures.MakeContext();

        var result = ir.Evaluate(context);

        result.Value.Should().Be(14);
    }

    [Fact]
    public void Evaluate_IR_HOT_HOT_ReturnsOneHundredNinetySix()
    {
        var ir = new IR(new HOT(), new HOT());
        var context = TestFixtures.MakeContext();

        var result = ir.Evaluate(context);

        result.Value.Should().Be(196);
    }

    [Fact]
    public void Evaluate_IR_HOT_IR_HOT_HOT_ReturnsTwoThousandSevenHundredFortyFour()
    {
        var ir = new IR(new HOT(), new IR(new HOT(), new HOT()));
        var context = TestFixtures.MakeContext();

        var result = ir.Evaluate(context);

        result.Value.Should().Be(2744);
    }
}
