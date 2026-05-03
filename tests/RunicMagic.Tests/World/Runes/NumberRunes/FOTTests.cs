using RunicMagic.World.Runes.NumberRunes;

namespace RunicMagic.Tests.World.Runes.NumberRunes;

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
