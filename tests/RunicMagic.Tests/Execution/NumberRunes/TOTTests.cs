using FluentAssertions;
using RunicMagic.World.Runes.NumberRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.NumberRunes;

public class TOTTests
{
    [Fact]
    public void Evaluate_ReturnsTwoThousandSevenHundredFortyFour()
    {
        var tot = new TOT();
        var context = TestFixtures.MakeContext();

        var result = tot.Evaluate(context);

        result.Value.Should().Be(2744);
    }
}
