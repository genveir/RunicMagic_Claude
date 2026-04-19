using FluentAssertions;
using RunicMagic.World.Runes.NumberRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.NumberRunes;

public class TETTests
{
    [Fact]
    public void Evaluate_ReturnsThree()
    {
        var tet = new TET();
        var context = TestFixtures.MakeContext();

        var result = tet.Evaluate(context);

        result.Value.Should().Be(3);
    }
}
