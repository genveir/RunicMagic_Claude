using FluentAssertions;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.EffectRunes;
using RunicMagic.World.Execution;
using RunicMagic.World.Runes.EffectRunes;
using RunicMagic.World.Runes.LocationRunes;
using RunicMagic.World.Runes.RuneTypes;
using Xunit;

namespace RunicMagic.Tests.RuneParsing.EffectRunes;

public class CJIRParserTests
{
    [Fact]
    public void ResolvesFromParserLookup()
    {
        var parser = ParserLookup.FindRuneParserByName<IStatement>("CJIR");

        parser.Should().BeOfType<CJIRParser>();
    }

    [Fact]
    public void Parse_WithAllExplicitArgs_ProducesCorrectCJIR()
    {
        var mockEntitySet = new MockEntitySet();
        var mockNumber = new MockNumber();
        var mockLocation = new MockLocation();
        ParserLookup.AddRuneParser("CJIR_HappyPath_IEntitySet", new MockParser<IEntitySet>(mockEntitySet));
        ParserLookup.AddRuneParser("CJIR_HappyPath_INumber", new MockParser<INumber>(mockNumber));
        ParserLookup.AddRuneParser("CJIR_HappyPath_ILocation", new MockParser<ILocation>(mockLocation));

        var result = new CJIRParser().Parse(new TokenStream("CJIR_HappyPath_IEntitySet CJIR_HappyPath_INumber CJIR_HappyPath_ILocation"));

        result.Succeeded.Should().BeTrue();
        var cjir = result.Value.Should().BeOfType<CJIR>().Subject;
        cjir.ToRotate.Should().BeOfType<EntitySetSelectionCostResolver>()
            .Which.Inner.Should().BeSameAs(mockEntitySet);
        cjir.HowMuch.Should().BeSameAs(mockNumber);
        cjir.Origin.Should().BeSameAs(mockLocation);
    }

    [Fact]
    public void Parse_WithDefaultOrigin_InjectsParOfTarget()
    {
        // When origin is omitted, the parser re-parses the target expression under PAR.
        // Both ToRotate and the PAR's inner set should wrap the same mock instance.
        var mockEntitySet = new MockEntitySet();
        var mockNumber = new MockNumber();
        ParserLookup.AddRuneParser("CJIR_DefaultOrigin_IEntitySet", new MockParser<IEntitySet>(mockEntitySet));
        ParserLookup.AddRuneParser("CJIR_DefaultOrigin_INumber", new MockParser<INumber>(mockNumber));

        var result = new CJIRParser().Parse(new TokenStream("CJIR_DefaultOrigin_IEntitySet CJIR_DefaultOrigin_INumber"));

        result.Succeeded.Should().BeTrue();
        var cjir = result.Value.Should().BeOfType<CJIR>().Subject;
        cjir.ToRotate.Should().BeOfType<EntitySetSelectionCostResolver>()
            .Which.Inner.Should().BeSameAs(mockEntitySet);
        var par = cjir.Origin.Should().BeOfType<PAR>().Subject;
        par.EntitySet.Should().BeOfType<EntitySetSelectionCostResolver>()
            .Which.Inner.Should().BeSameAs(mockEntitySet);
    }

    [Fact]
    public void Parse_WithMissingEntitySet_Fails()
    {
        var result = new CJIRParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public void Parse_WithMissingNumber_Fails()
    {
        var mockEntitySet = new MockEntitySet();
        ParserLookup.AddRuneParser("CJIR_MissingNumber_IEntitySet", new MockParser<IEntitySet>(mockEntitySet));

        var result = new CJIRParser().Parse(new TokenStream("CJIR_MissingNumber_IEntitySet"));

        result.Succeeded.Should().BeFalse();
    }
}
