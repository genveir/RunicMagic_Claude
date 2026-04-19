using FluentAssertions;
using RunicMagic.World.Runes.NumberRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.ArithmeticRunes;

public class MOTests
{
    [Fact]
    public void Evaluate_MO_HET_HET_ReturnsTwo()
    {
        var mo = new MO(new HET(), new HET());
        var context = TestFixtures.MakeContext();

        var result = mo.Evaluate(context);

        result.Value.Should().Be(2);
    }

    [Fact]
    public void Evaluate_MO_HOT_HET_ReturnsFifteen()
    {
        var mo = new MO(new HOT(), new HET());
        var context = TestFixtures.MakeContext();

        var result = mo.Evaluate(context);

        result.Value.Should().Be(15);
    }

    [Fact]
    public void Evaluate_MO_HOT_HOT_ReturnsTwentyEight()
    {
        var mo = new MO(new HOT(), new HOT());
        var context = TestFixtures.MakeContext();

        var result = mo.Evaluate(context);

        result.Value.Should().Be(28);
    }
}
