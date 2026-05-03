using RunicMagic.World.Runes.NumberRunes;

namespace RunicMagic.Tests.World.Runes.NumberRunes;

public class HETTests
{
    [Fact]
    public void Evaluate_ReturnsOne()
    {
        var het = new HET();
        var context = TestFixtures.MakeContext();

        var result = het.Evaluate(context);

        result.Value.Should().Be(1);
    }
}
