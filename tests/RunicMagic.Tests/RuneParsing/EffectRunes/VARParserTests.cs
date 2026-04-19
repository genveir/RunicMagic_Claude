using FluentAssertions;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.EffectRunes;
using RunicMagic.World.Execution;
using RunicMagic.World.Runes.EffectRunes;
using RunicMagic.World.Runes.EntityReferenceRunes;
using RunicMagic.World.Runes.LocationRunes;
using RunicMagic.World.Runes.RuneTypes;
using Xunit;

namespace RunicMagic.Tests.RuneParsing.EffectRunes;

public class VARParserTests
{
    [Fact]
    public void ResolvesFromParserLookup()
    {
        var parser = ParserLookup.FindRuneParserByName<IStatement>("VAR");

        parser.Should().BeOfType<VARParser>();
    }

    [Fact]
    public void Parse_WithAllExplicitArgs_ProducesCorrectVAR()
    {
        var mockEntitySet = new MockEntitySet();
        var mockNumber = new MockNumber();
        var mockLocation = new MockLocation();
        ParserLookup.AddRuneParser("VAR_HappyPath_IEntitySet", new MockParser<IEntitySet>(mockEntitySet));
        ParserLookup.AddRuneParser("VAR_HappyPath_INumber", new MockParser<INumber>(mockNumber));
        ParserLookup.AddRuneParser("VAR_HappyPath_ILocation", new MockParser<ILocation>(mockLocation));

        var result = new VARParser().Parse(new TokenStream("VAR_HappyPath_IEntitySet VAR_HappyPath_INumber VAR_HappyPath_ILocation"));

        result.Succeeded.Should().BeTrue();
        var var_ = result.Value.Should().BeOfType<VAR>().Subject;
        var_.ToMove.Should().BeOfType<EntitySetSelectionCostResolver>()
            .Which.Inner.Should().BeSameAs(mockEntitySet);
        var_.HowFar.Should().BeSameAs(mockNumber);
        var_.Origin.Should().BeSameAs(mockLocation);
    }

    [Fact]
    public void Parse_WithDefaultLocation_InjectsParOH()
    {
        var mockEntitySet = new MockEntitySet();
        var mockNumber = new MockNumber();
        ParserLookup.AddRuneParser("VAR_DefaultLocation_IEntitySet", new MockParser<IEntitySet>(mockEntitySet));
        ParserLookup.AddRuneParser("VAR_DefaultLocation_INumber", new MockParser<INumber>(mockNumber));

        var result = new VARParser().Parse(new TokenStream("VAR_DefaultLocation_IEntitySet VAR_DefaultLocation_INumber"));

        result.Succeeded.Should().BeTrue();
        var var_ = result.Value.Should().BeOfType<VAR>().Subject;
        var par = var_.Origin.Should().BeOfType<PAR>().Subject;
        par.EntitySet.Should().BeOfType<EntitySetSelectionCostResolver>()
            .Which.Inner.Should().BeOfType<OH>();
    }

    [Fact]
    public void Parse_WithMissingEntitySet_Fails()
    {
        var result = new VARParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public void Parse_WithMissingNumber_Fails()
    {
        var mockEntitySet = new MockEntitySet();
        ParserLookup.AddRuneParser("VAR_MissingNumber_IEntitySet", new MockParser<IEntitySet>(mockEntitySet));

        var result = new VARParser().Parse(new TokenStream("VAR_MissingNumber_IEntitySet"));

        result.Succeeded.Should().BeFalse();
    }
}
