using RunicMagic.World.Runes.NumberRunes;

namespace RunicMagic.Tests.World.Runes.NumberRunes;

public class DOTTests
{
    [Fact]
    public void Evaluate_ReturnsOneHundredNinetySix()
    {
        var dot = new DOT();
        var context = TestFixtures.MakeContext();

        var result = dot.Evaluate(context);

        result.Value.Should().Be(196);
    }
}
