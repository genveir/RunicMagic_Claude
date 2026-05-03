using RunicMagic.World.Runes.NumberRunes;

namespace RunicMagic.Tests.World.Runes.NumberRunes;

public class SOTTests
{
    [Fact]
    public void Evaluate_ReturnsOneHundredFiveMillionFourHundredThirteenThousandFiveHundredFour()
    {
        var sot = new SOT();
        var context = TestFixtures.MakeContext();

        var result = sot.Evaluate(context);

        result.Value.Should().Be(105413504);
    }
}
