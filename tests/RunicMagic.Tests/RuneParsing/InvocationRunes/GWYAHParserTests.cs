using FluentAssertions;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.World.Execution;
using RunicMagic.World.Runes.EntityReferenceRunes;
using RunicMagic.World.Runes.ExecutionRunes;
using RunicMagic.World.Runes.InvocationRunes;
using Xunit;

namespace RunicMagic.Tests.RuneParsing.InvocationRunes;

public class GWYAHParserTests
{
    [Fact]
    public void Parse_GwyahA_ProducesGwyahWithATarget()
    {
        var (_, result) = SpellParser.Parse("ZU GWYAH A");

        result.Succeeded.Should().BeTrue();
        var zu = result.Value.Should().BeOfType<ZU>().Subject;
        var gwyah = zu.Statement.Should().BeOfType<GWYAH>().Subject;
        gwyah.Target.Should().BeOfType<EntitySetSelectionCostResolver>()
            .Which.Inner.Should().BeOfType<A>();
    }

    [Fact]
    public void Parse_GwyahDan_ProducesGwyahWithDanTarget()
    {
        var (_, result) = SpellParser.Parse("ZU GWYAH DAN");

        result.Succeeded.Should().BeTrue();
        var zu = result.Value.Should().BeOfType<ZU>().Subject;
        var gwyah = zu.Statement.Should().BeOfType<GWYAH>().Subject;
        gwyah.Target.Should().BeOfType<EntitySetSelectionCostResolver>()
            .Which.Inner.Should().BeOfType<DAN>();
    }

    [Fact]
    public void ParseAsStatement_GwyahOh_Succeeds()
    {
        var result = SpellParser.ParseAsStatement("GWYAH OH");

        result.Should().BeOfType<GWYAH>();
    }

    [Fact]
    public void Parse_GwyahWithNoArgument_Fails()
    {
        var (_, result) = SpellParser.Parse("ZU GWYAH");

        result.Succeeded.Should().BeFalse();
    }
}
