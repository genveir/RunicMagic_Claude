using FluentAssertions;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.EntitySetRunes;
using RunicMagic.Tests.RuneParsing;
using RunicMagic.World.Execution;
using RunicMagic.World.Runes.EntityReferenceRunes;
using RunicMagic.World.Runes.EntitySetRunes;
using RunicMagic.World.Runes.LocationRunes;
using RunicMagic.World.Runes.RuneTypes;
using Xunit;

namespace RunicMagic.Tests.RuneParsing.EntitySetRunes;

public class HOROParserTests
{
    [Fact]
    public void ResolvesFromParserLookup()
    {
        var parser = ParserLookup.FindRuneParserByName<IEntitySet>("HORO");

        parser.Should().BeOfType<HOROParser>();
    }

    [Fact]
    public void Parse_WithAllExplicitArgs_ProducesCorrectHORO()
    {
        var mockNumber = new MockNumber();
        var mockLocation = new MockLocation();
        ParserLookup.AddRuneParser("HORO_HappyPath_INumber", new MockParser<INumber>(mockNumber));
        ParserLookup.AddRuneParser("HORO_HappyPath_ILocation", new MockParser<ILocation>(mockLocation));

        var result = new HOROParser().Parse(new TokenStream("HORO_HappyPath_INumber HORO_HappyPath_ILocation"));

        result.Succeeded.Should().BeTrue();
        var horo = result.Value.Should().BeOfType<HORO>().Subject;
        horo.HowFar.Should().BeSameAs(mockNumber);
        horo.Origin.Should().BeSameAs(mockLocation);
    }

    [Fact]
    public void Parse_WithDefaultOrigin_InjectsParOH()
    {
        var mockNumber = new MockNumber();
        ParserLookup.AddRuneParser("HORO_DefaultOrigin_INumber", new MockParser<INumber>(mockNumber));

        var result = new HOROParser().Parse(new TokenStream("HORO_DefaultOrigin_INumber"));

        result.Succeeded.Should().BeTrue();
        var horo = result.Value.Should().BeOfType<HORO>().Subject;
        horo.HowFar.Should().BeSameAs(mockNumber);
        var par = horo.Origin.Should().BeOfType<PAR>().Subject;
        par.EntitySet.Should().BeOfType<EntitySetSelectionCostResolver>()
            .Which.Inner.Should().BeOfType<OH>();
    }

    [Fact]
    public void Parse_WithMissingNumber_Fails()
    {
        var result = new HOROParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeFalse();
    }
}
