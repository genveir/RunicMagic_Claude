using FluentAssertions;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.LocationRunes;
using RunicMagic.Tests.RuneParsing;
using RunicMagic.World.Runes.LocationRunes;
using RunicMagic.World.Runes.RuneTypes;
using Xunit;

namespace RunicMagic.Tests.RuneParsing.LocationRunes;

public class GERParserTests
{
    [Fact]
    public void ResolvesFromParserLookup()
    {
        var parser = ParserLookup.FindRuneParserByName<ILocation>("GER");

        parser.Should().BeOfType<GERParser>();
    }

    [Fact]
    public void Parse_WithEntitySet_WrapsInGER()
    {
        var mockEntitySet = new MockEntitySet();
        ParserLookup.AddRuneParser("GER_HappyPath_IEntitySet", new MockParser<IEntitySet>(mockEntitySet));

        var result = new GERParser().Parse(new TokenStream("GER_HappyPath_IEntitySet"));

        result.Succeeded.Should().BeTrue();
        var ger = result.Value.Should().BeOfType<GER>().Subject;
        ger.EntitySet.Should().BeSameAs(mockEntitySet);
    }

    [Fact]
    public void Parse_WithEmptyStream_Fails()
    {
        var result = new GERParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeFalse();
    }
}
