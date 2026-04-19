using FluentAssertions;
using RunicMagic.World.Runes.NumberRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.NumberRunes;

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
