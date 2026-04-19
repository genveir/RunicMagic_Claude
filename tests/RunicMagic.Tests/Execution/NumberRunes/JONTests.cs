using FluentAssertions;
using RunicMagic.World.Runes.NumberRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.NumberRunes;

public class JONTests
{
    [Fact]
    public void Evaluate_ReturnsZero()
    {
        var jon = new JON();
        var context = TestFixtures.MakeContext();

        var result = jon.Evaluate(context);

        result.Value.Should().Be(0);
    }
}
