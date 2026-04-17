using FluentAssertions;
using RunicMagic.World.Runes.NumberRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.NumberRunes;

public class FOTIRTests
{
    [Fact]
    public void Evaluate_FOTIR_HET_ReturnsFourteen()
    {
        var fotir = new FOTIR(new HET());
        var context = TestFixtures.MakeContext();

        var result = fotir.Evaluate(context);

        result.Value.Should().Be(14);
    }

    [Fact]
    public void Evaluate_FOTIR_FOTIR_HET_ReturnsOneHundredNinetySix()
    {
        var fotir = new FOTIR(new FOTIR(new HET()));
        var context = TestFixtures.MakeContext();

        var result = fotir.Evaluate(context);

        result.Value.Should().Be(196);
    }

    [Fact]
    public void Evaluate_FOTIR_FOTIR_FOTIR_HET_ReturnsTwoThousandSevenHundredFortyFour()
    {
        var fotir = new FOTIR(new FOTIR(new FOTIR(new HET())));
        var context = TestFixtures.MakeContext();

        var result = fotir.Evaluate(context);

        result.Value.Should().Be(2744);
    }
}
