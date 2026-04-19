using FluentAssertions;
using RunicMagic.World.Runes.NumberRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.NumberRunes;

public class FOTTests
{
    [Fact]
    public void Evaluate_ReturnsFiveHundredThirtySevenThousandEightHundredTwentyFour()
    {
        var fot = new FOT();
        var context = TestFixtures.MakeContext();

        var result = fot.Evaluate(context);

        result.Value.Should().Be(537824);
    }
}
