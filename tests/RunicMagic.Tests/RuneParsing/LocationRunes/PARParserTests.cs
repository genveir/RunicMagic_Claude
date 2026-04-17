using FluentAssertions;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.LocationRunes;
using RunicMagic.Tests.RuneParsing;
using RunicMagic.World.Runes.LocationRunes;
using RunicMagic.World.Runes.RuneTypes;
using Xunit;

namespace RunicMagic.Tests.RuneParsing.LocationRunes;

public class PARParserTests
{
    [Fact]
    public void ResolvesFromParserLookup()
    {
        var parser = ParserLookup.FindRuneParserByName<ILocation>("PAR");

        parser.Should().BeOfType<PARParser>();
    }

    [Fact]
    public void Parse_WithEntitySet_WrapsInPAR()
    {
        var mockEntitySet = new MockEntitySet();
        ParserLookup.AddRuneParser("PAR_HappyPath_IEntitySet", new MockParser<IEntitySet>(mockEntitySet));

        var result = new PARParser().Parse(new TokenStream("PAR_HappyPath_IEntitySet"));

        result.Succeeded.Should().BeTrue();
        var par = result.Value.Should().BeOfType<PAR>().Subject;
        par.EntitySet.Should().BeSameAs(mockEntitySet);
    }

    [Fact]
    public void Parse_WithEmptyStream_Fails()
    {
        var result = new PARParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeFalse();
    }
}
