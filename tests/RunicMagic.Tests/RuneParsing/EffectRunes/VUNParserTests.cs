using FluentAssertions;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.EffectRunes;
using RunicMagic.World.Runes.EffectRunes;
using RunicMagic.World.Runes.EntityReferenceRunes;
using RunicMagic.World.Runes.LocationRunes;
using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;
using Xunit;

namespace RunicMagic.Tests.RuneParsing.EffectRunes;

public class VUNParserTests
{
    [Fact]
    public void ResolvesFromParserLookup()
    {
        var parser = ParserLookup.FindRuneParserByName<IStatement>("VUN");

        parser.Should().BeOfType<VUNParser>();
    }

    [Fact]
    public void Parse_WithAllExplicitArgs_ProducesCorrectVUN()
    {
        var mockEntitySet = new MockEntitySet();
        var mockNumber = new MockNumber();
        var mockLocation = new MockLocation();
        ParserLookup.AddRuneParser("VUN_HappyPath_IEntitySet", new MockParser<IEntitySet>(mockEntitySet));
        ParserLookup.AddRuneParser("VUN_HappyPath_INumber", new MockParser<INumber>(mockNumber));
        ParserLookup.AddRuneParser("VUN_HappyPath_ILocation", new MockParser<ILocation>(mockLocation));

        var result = new VUNParser().Parse(new TokenStream("VUN_HappyPath_IEntitySet VUN_HappyPath_INumber VUN_HappyPath_ILocation"));

        result.Succeeded.Should().BeTrue();
        var vun = result.Value.Should().BeOfType<VUN>().Subject;
        vun.ToMove.Should().BeOfType<EntitySetSelectionCostResolver>()
            .Which.Inner.Should().BeSameAs(mockEntitySet);
        vun.HowFar.Should().BeSameAs(mockNumber);
        vun.Origin.Should().BeSameAs(mockLocation);
    }

    [Fact]
    public void Parse_WithDefaultLocation_InjectsParA()
    {
        var mockEntitySet = new MockEntitySet();
        var mockNumber = new MockNumber();
        ParserLookup.AddRuneParser("VUN_DefaultLocation_IEntitySet", new MockParser<IEntitySet>(mockEntitySet));
        ParserLookup.AddRuneParser("VUN_DefaultLocation_INumber", new MockParser<INumber>(mockNumber));

        var result = new VUNParser().Parse(new TokenStream("VUN_DefaultLocation_IEntitySet VUN_DefaultLocation_INumber"));

        result.Succeeded.Should().BeTrue();
        var vun = result.Value.Should().BeOfType<VUN>().Subject;
        var par = vun.Origin.Should().BeOfType<PAR>().Subject;
        par.EntitySet.Should().BeOfType<EntitySetSelectionCostResolver>()
            .Which.Inner.Should().BeOfType<A>();
    }

    [Fact]
    public void Parse_WithMissingEntitySet_Fails()
    {
        var result = new VUNParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public void Parse_WithMissingNumber_Fails()
    {
        var mockEntitySet = new MockEntitySet();
        ParserLookup.AddRuneParser("VUN_MissingNumber_IEntitySet", new MockParser<IEntitySet>(mockEntitySet));

        var result = new VUNParser().Parse(new TokenStream("VUN_MissingNumber_IEntitySet"));

        result.Succeeded.Should().BeFalse();
    }
}
