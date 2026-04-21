using FluentAssertions;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.EffectRunes;
using RunicMagic.World.Execution;
using RunicMagic.World.Runes.EffectRunes;
using RunicMagic.World.Runes.LocationRunes;
using RunicMagic.World.Runes.RuneTypes;
using Xunit;

namespace RunicMagic.Tests.RuneParsing.EffectRunes;

public class CJARParserTests
{
    [Fact]
    public void ResolvesFromParserLookup()
    {
        var parser = ParserLookup.FindRuneParserByName<IStatement>("CJAR");

        parser.Should().BeOfType<CJARParser>();
    }

    [Fact]
    public void Parse_WithAllExplicitArgs_ProducesCorrectCJAR()
    {
        var mockEntitySet = new MockEntitySet();
        var mockNumber = new MockNumber();
        var mockLocation = new MockLocation();
        ParserLookup.AddRuneParser("CJAR_HappyPath_IEntitySet", new MockParser<IEntitySet>(mockEntitySet));
        ParserLookup.AddRuneParser("CJAR_HappyPath_INumber", new MockParser<INumber>(mockNumber));
        ParserLookup.AddRuneParser("CJAR_HappyPath_ILocation", new MockParser<ILocation>(mockLocation));

        var result = new CJARParser().Parse(new TokenStream("CJAR_HappyPath_IEntitySet CJAR_HappyPath_INumber CJAR_HappyPath_ILocation"));

        result.Succeeded.Should().BeTrue();
        var cjar = result.Value.Should().BeOfType<CJAR>().Subject;
        cjar.ToRotate.Should().BeOfType<EntitySetSelectionCostResolver>()
            .Which.Inner.Should().BeSameAs(mockEntitySet);
        cjar.HowMuch.Should().BeSameAs(mockNumber);
        cjar.Origin.Should().BeSameAs(mockLocation);
    }

    [Fact]
    public void Parse_WithDefaultOrigin_InjectsParOfTarget()
    {
        // When origin is omitted, the parser re-parses the target expression under PAR.
        // Both ToRotate and the PAR's inner set should wrap the same mock instance.
        var mockEntitySet = new MockEntitySet();
        var mockNumber = new MockNumber();
        ParserLookup.AddRuneParser("CJAR_DefaultOrigin_IEntitySet", new MockParser<IEntitySet>(mockEntitySet));
        ParserLookup.AddRuneParser("CJAR_DefaultOrigin_INumber", new MockParser<INumber>(mockNumber));

        var result = new CJARParser().Parse(new TokenStream("CJAR_DefaultOrigin_IEntitySet CJAR_DefaultOrigin_INumber"));

        result.Succeeded.Should().BeTrue();
        var cjar = result.Value.Should().BeOfType<CJAR>().Subject;
        cjar.ToRotate.Should().BeOfType<EntitySetSelectionCostResolver>()
            .Which.Inner.Should().BeSameAs(mockEntitySet);
        var par = cjar.Origin.Should().BeOfType<PAR>().Subject;
        par.EntitySet.Should().BeOfType<EntitySetSelectionCostResolver>()
            .Which.Inner.Should().BeSameAs(mockEntitySet);
    }

    [Fact]
    public void Parse_WithMissingEntitySet_Fails()
    {
        var result = new CJARParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public void Parse_WithMissingNumber_Fails()
    {
        var mockEntitySet = new MockEntitySet();
        ParserLookup.AddRuneParser("CJAR_MissingNumber_IEntitySet", new MockParser<IEntitySet>(mockEntitySet));

        var result = new CJARParser().Parse(new TokenStream("CJAR_MissingNumber_IEntitySet"));

        result.Succeeded.Should().BeFalse();
    }
}
