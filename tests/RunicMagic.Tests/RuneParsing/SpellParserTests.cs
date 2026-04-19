using FluentAssertions;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.World.Execution;
using RunicMagic.World.Runes.EffectRunes;
using RunicMagic.World.Runes.EntityReferenceRunes;
using RunicMagic.World.Runes.EntitySetRunes;
using RunicMagic.World.Runes.ExecutionRunes;
using RunicMagic.World.Runes.LocationRunes;
using RunicMagic.World.Runes.NumberRunes;
using Xunit;

namespace RunicMagic.Tests.RuneParsing;

public class SpellParserTests
{
    [Fact]
    public void Parse_ZuVunAHet_ProducesCorrectTree()
    {
        var (count, result) = SpellParser.Parse("ZU VUN A HET");

        count.Should().Be(6);
        result.Succeeded.Should().BeTrue();

        var zu = result.Value.Should().BeOfType<ZU>().Subject;
        var vun = zu.Statement.Should().BeOfType<VUN>().Subject;

        vun.ToMove.Should().BeOfType<EntitySetSelectionCostResolver>()
            .Which.Inner.Should().BeOfType<A>();
        vun.HowFar.Should().BeOfType<HET>();

        var par = vun.Origin.Should().BeOfType<PAR>().Subject;
        par.EntitySet.Should().BeOfType<EntitySetSelectionCostResolver>()
            .Which.Inner.Should().BeOfType<OH>();
    }

    [Fact]
    public void Parse_MilestoneSpell_ProducesCorrectTree()
    {
        var (count, result) = SpellParser.Parse("ZU VUN LA IR HOT IR HOT HOT");

        count.Should().Be(11);
        result.Succeeded.Should().BeTrue();

        var zu = result.Value.Should().BeOfType<ZU>().Subject;
        var vun = zu.Statement.Should().BeOfType<VUN>().Subject;

        var la = vun.ToMove.Should().BeOfType<EntitySetSelectionCostResolver>()
            .Which.Inner.Should().BeOfType<LA>().Subject;
        la.ToGetScopeOf.Should().BeOfType<OH>();

        var ir1 = vun.HowFar.Should().BeOfType<IR>().Subject;
        ir1.A.Should().BeOfType<HOT>();
        var ir2 = ir1.B.Should().BeOfType<IR>().Subject;
        ir2.A.Should().BeOfType<HOT>();
        ir2.B.Should().BeOfType<HOT>();

        var par = vun.Origin.Should().BeOfType<PAR>().Subject;
        par.EntitySet.Should().BeOfType<EntitySetSelectionCostResolver>()
            .Which.Inner.Should().BeOfType<OH>();
    }

    [Fact]
    public void Parse_EmptyString_Fails()
    {
        var (_, result) = SpellParser.Parse("");

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public void Parse_WhitespaceString_Fails()
    {
        var (_, result) = SpellParser.Parse("   ");

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public void Parse_WithInvalidRune_Fails()
    {
        var (_, result) = SpellParser.Parse("ZU NOTARUNE");

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public void Parse_WithEndOfStream_Fails()
    {
        var (_, result) = SpellParser.Parse("ZU VUN A");

        result.Succeeded.Should().BeFalse();
    }

    // ── ParseAsStatement ──────────────────────────────────────────────────────

    [Fact]
    public void ParseAsStatement_ValidStatement_ReturnsStatement()
    {
        var result = SpellParser.ParseAsStatement("VUN A HET");

        result.Should().BeOfType<VUN>();
    }

    [Fact]
    public void ParseAsStatement_InvalidRune_ReturnsNull()
    {
        var result = SpellParser.ParseAsStatement("NOTARUNE");

        result.Should().BeNull();
    }

    [Fact]
    public void ParseAsStatement_EmptyString_ReturnsNull()
    {
        var result = SpellParser.ParseAsStatement("");

        result.Should().BeNull();
    }

    [Fact]
    public void ParseAsStatement_ExecutionRune_ReturnsNull()
    {
        // ZU produces IExecutableStatement, not IStatement — should fail to parse as Statement
        var result = SpellParser.ParseAsStatement("ZU VUN A HET");

        result.Should().BeNull();
    }
}
