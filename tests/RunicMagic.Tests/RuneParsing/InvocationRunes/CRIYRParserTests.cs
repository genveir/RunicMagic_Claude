using FluentAssertions;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.World.Execution;
using RunicMagic.World.Runes.EntityReferenceRunes;
using RunicMagic.World.Runes.ExecutionRunes;
using RunicMagic.World.Runes.InvocationRunes;
using Xunit;

namespace RunicMagic.Tests.RuneParsing.InvocationRunes;

public class CRIYRParserTests
{
    [Fact]
    public void Parse_CriyrA_ProducesCriyrWithATarget()
    {
        var (_, result) = SpellParser.Parse("ZU CRIYR A");

        result.Succeeded.Should().BeTrue();
        var zu = result.Value.Should().BeOfType<ZU>().Subject;
        var criyr = zu.Statement.Should().BeOfType<CRIYR>().Subject;
        criyr.Target.Should().BeOfType<EntitySetSelectionCostResolver>()
            .Which.Inner.Should().BeOfType<A>();
    }

    [Fact]
    public void Parse_CriyrDan_ProducesCriyrWithDanTarget()
    {
        var (_, result) = SpellParser.Parse("ZU CRIYR DAN");

        result.Succeeded.Should().BeTrue();
        var zu = result.Value.Should().BeOfType<ZU>().Subject;
        var criyr = zu.Statement.Should().BeOfType<CRIYR>().Subject;
        criyr.Target.Should().BeOfType<EntitySetSelectionCostResolver>()
            .Which.Inner.Should().BeOfType<DAN>();
    }

    [Fact]
    public void ParseAsStatement_CriyrOh_Succeeds()
    {
        var result = SpellParser.ParseAsStatement("CRIYR OH");

        result.Should().BeOfType<CRIYR>();
    }

    [Fact]
    public void Parse_CriyrWithNoArgument_Fails()
    {
        var (_, result) = SpellParser.Parse("ZU CRIYR");

        result.Succeeded.Should().BeFalse();
    }
}
