using FluentAssertions;
using RunicMagic.World.Runes.NumberRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.NumberRunes;

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
