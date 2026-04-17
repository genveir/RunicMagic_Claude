using FluentAssertions;
using RunicMagic.Controller.RuneParsing;
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

        vun.ToMove.Should().BeOfType<A>();
        vun.HowFar.Should().BeOfType<HET>();

        var par = vun.Origin.Should().BeOfType<PAR>().Subject;
        par.EntitySet.Should().BeOfType<A>();
    }

    [Fact]
    public void Parse_MilestoneSpell_ProducesCorrectTree()
    {
        var (count, result) = SpellParser.Parse("ZU VUN LA FOTIR FOTIR FOTIR HET");

        count.Should().Be(10);
        result.Succeeded.Should().BeTrue();

        var zu = result.Value.Should().BeOfType<ZU>().Subject;
        var vun = zu.Statement.Should().BeOfType<VUN>().Subject;

        var la = vun.ToMove.Should().BeOfType<LA>().Subject;
        la.ToGetScopeOf.Should().BeOfType<OH>();

        var fotir1 = vun.HowFar.Should().BeOfType<FOTIR>().Subject;
        var fotir2 = fotir1.Multiplicand.Should().BeOfType<FOTIR>().Subject;
        var fotir3 = fotir2.Multiplicand.Should().BeOfType<FOTIR>().Subject;
        fotir3.Multiplicand.Should().BeOfType<HET>();

        var par = vun.Origin.Should().BeOfType<PAR>().Subject;
        par.EntitySet.Should().BeOfType<A>();
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
}
