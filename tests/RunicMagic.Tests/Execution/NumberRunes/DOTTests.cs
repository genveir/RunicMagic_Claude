using FluentAssertions;
using RunicMagic.World.Runes.NumberRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.NumberRunes;

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
