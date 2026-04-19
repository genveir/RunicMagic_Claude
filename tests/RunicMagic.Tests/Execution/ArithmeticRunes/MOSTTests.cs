using FluentAssertions;
using RunicMagic.World.Runes.NumberRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.ArithmeticRunes;

public class MOSTTests
{
    [Fact]
    public void Evaluate_MOST_DET_ReturnsThree()
    {
        var most = new MOST(new DET());
        var context = TestFixtures.MakeContext();

        var result = most.Evaluate(context);

        result.Value.Should().Be(3);
    }

    [Fact]
    public void Evaluate_MOST_HOT_ReturnsTwentyOne()
    {
        var most = new MOST(new HOT());
        var context = TestFixtures.MakeContext();

        var result = most.Evaluate(context);

        result.Value.Should().Be(21);
    }

    [Fact]
    public void Evaluate_MOST_TOT_ReturnsFourThousandOneHundredSixteen()
    {
        var most = new MOST(new TOT());
        var context = TestFixtures.MakeContext();

        var result = most.Evaluate(context);

        result.Value.Should().Be(4116);
    }

    [Fact]
    public void Evaluate_OddNumber_TruncatesTowardZero()
    {
        // 7 * 1.5 = 10.5, truncated to 10
        var most = new MOST(new SET());
        var context = TestFixtures.MakeContext();

        var result = most.Evaluate(context);

        result.Value.Should().Be(10);
    }
}
