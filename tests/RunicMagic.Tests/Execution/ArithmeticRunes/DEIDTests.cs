using FluentAssertions;
using RunicMagic.World.Runes.NumberRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.ArithmeticRunes;

public class DEIDTests
{
    [Fact]
    public void Evaluate_DEID_DET_ReturnsOne()
    {
        var deid = new DEID(new DET());
        var context = TestFixtures.MakeContext();

        var result = deid.Evaluate(context);

        result.Value.Should().Be(1);
    }

    [Fact]
    public void Evaluate_DEID_HOT_ReturnsSeven()
    {
        var deid = new DEID(new HOT());
        var context = TestFixtures.MakeContext();

        var result = deid.Evaluate(context);

        result.Value.Should().Be(7);
    }

    [Fact]
    public void Evaluate_DEID_TOT_ReturnsOneThousandThreeHundredSeventyTwo()
    {
        var deid = new DEID(new TOT());
        var context = TestFixtures.MakeContext();

        var result = deid.Evaluate(context);

        result.Value.Should().Be(1372);
    }

    [Fact]
    public void Evaluate_OddNumber_TruncatesTowardZero()
    {
        // 7 / 2 = 3 (truncated)
        var deid = new DEID(new SET());
        var context = TestFixtures.MakeContext();

        var result = deid.Evaluate(context);

        result.Value.Should().Be(3);
    }
}
