using RunicMagic.World.Runes.NumberRunes;

namespace RunicMagic.Tests.Execution.NumberRunes;

public class DETTests
{
    [Fact]
    public void Evaluate_ReturnsTwo()
    {
        var det = new DET();
        var context = TestFixtures.MakeContext();

        var result = det.Evaluate(context);

        result.Value.Should().Be(2);
    }
}
