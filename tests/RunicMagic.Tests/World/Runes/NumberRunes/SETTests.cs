using RunicMagic.World.Runes.NumberRunes;

namespace RunicMagic.Tests.World.Runes.NumberRunes;

public class SETTests
{
    [Fact]
    public void Evaluate_ReturnsSeven()
    {
        var set = new SET();
        var context = TestFixtures.MakeContext();

        var result = set.Evaluate(context);

        result.Value.Should().Be(7);
    }
}
