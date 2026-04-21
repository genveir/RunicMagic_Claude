using FluentAssertions;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.FilterRunes;
using RunicMagic.Tests.RuneParsing;
using RunicMagic.World.Runes.FilterRunes;
using RunicMagic.World.Runes.RuneTypes;
using Xunit;

namespace RunicMagic.Tests.RuneParsing.FilterRunes;

public class ZYSEParserTests
{
    [Fact]
    public void ResolvesFromParserLookup()
    {
        var parser = ParserLookup.FindRuneParserByName<IEntitySet>("ZYSE");

        parser.Should().BeOfType<ZYSEParser>();
    }

    [Fact]
    public void Parse_WithSource_ProducesCorrectZYSE()
    {
        var mockSource = new MockEntitySet();
        ParserLookup.AddRuneParser("ZYSE_HappyPath_IEntitySet", new MockParser<IEntitySet>(mockSource));

        var result = new ZYSEParser().Parse(new TokenStream("ZYSE_HappyPath_IEntitySet"));

        result.Succeeded.Should().BeTrue();
        var zyse = result.Value.Should().BeOfType<ZYSE>().Subject;
        zyse.Source.Should().BeSameAs(mockSource);
    }

    [Fact]
    public void Parse_WithMissingSource_Fails()
    {
        var result = new ZYSEParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeFalse();
    }
}
