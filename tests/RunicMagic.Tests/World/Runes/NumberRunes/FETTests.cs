using RunicMagic.World.Runes.NumberRunes;

namespace RunicMagic.Tests.World.Runes.NumberRunes;

public class FETTests
{
    [Fact]
    public void Evaluate_ReturnsFive()
    {
        var fet = new FET();
        var context = TestFixtures.MakeContext();

        var result = fet.Evaluate(context);

        result.Value.Should().Be(5);
    }
}
