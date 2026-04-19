using FluentAssertions;
using RunicMagic.World.Runes.NumberRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.ArithmeticRunes;

public class UITTests
{
    [Fact]
    public void Evaluate_UIT_HOT_HET_ReturnsZero()
    {
        var uit = new UIT(new HOT(), new HET());
        var context = TestFixtures.MakeContext();

        var result = uit.Evaluate(context);

        result.Value.Should().Be(0);
    }

    [Fact]
    public void Evaluate_UIT_HOT_HOT_ReturnsZero()
    {
        var uit = new UIT(new HOT(), new HOT());
        var context = TestFixtures.MakeContext();

        var result = uit.Evaluate(context);

        result.Value.Should().Be(0);
    }

    [Fact]
    public void Evaluate_NonCommutative_AModB_DiffersFromBModA()
    {
        // 14 % 3 = 2, but 3 % 14 = 3 — order matters
        var three = new MO(new MO(new HET(), new HET()), new HET());
        var uitAB = new UIT(new HOT(), three);
        var uitBA = new UIT(three, new HOT());
        var context = TestFixtures.MakeContext();

        var ab = uitAB.Evaluate(context);
        var ba = uitBA.Evaluate(context);

        ab.Value.Should().Be(2);
        ba.Value.Should().Be(3);
    }
}
