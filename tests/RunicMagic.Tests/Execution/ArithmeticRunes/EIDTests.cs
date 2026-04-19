using FluentAssertions;
using RunicMagic.World.Runes.NumberRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.ArithmeticRunes;

public class EIDTests
{
    [Fact]
    public void Evaluate_EID_HOT_HET_ReturnsFourteen()
    {
        var eid = new EID(new HOT(), new HET());
        var context = TestFixtures.MakeContext();

        var result = eid.Evaluate(context);

        result.Value.Should().Be(14);
    }

    [Fact]
    public void Evaluate_EID_HOT_HOT_ReturnsOne()
    {
        var eid = new EID(new HOT(), new HOT());
        var context = TestFixtures.MakeContext();

        var result = eid.Evaluate(context);

        result.Value.Should().Be(1);
    }

    [Fact]
    public void Evaluate_TruncatesTowardZero()
    {
        // IR HOT HET = 15; EID 15 HOT = 15 / 14 = 1 (truncated)
        var fifteen = new MO(new HOT(), new HET());
        var eid = new EID(fifteen, new HOT());
        var context = TestFixtures.MakeContext();

        var result = eid.Evaluate(context);

        result.Value.Should().Be(1);
    }

    [Fact]
    public void Evaluate_NonCommutative_ADivB_DiffersFromBDivA()
    {
        // 14 / 1 = 14, but 1 / 14 = 0
        var eidAB = new EID(new HOT(), new HET());
        var eidBA = new EID(new HET(), new HOT());
        var context = TestFixtures.MakeContext();

        var ab = eidAB.Evaluate(context);
        var ba = eidBA.Evaluate(context);

        ab.Value.Should().Be(14);
        ba.Value.Should().Be(0);
    }
}
