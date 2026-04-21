using FluentAssertions;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.FilterRunes;
using RunicMagic.Tests.RuneParsing;
using RunicMagic.World.Runes.FilterRunes;
using RunicMagic.World.Runes.RuneTypes;
using Xunit;

namespace RunicMagic.Tests.RuneParsing.FilterRunes;

public class FUHEParserTests
{
    [Fact]
    public void ResolvesFromParserLookup()
    {
        var parser = ParserLookup.FindRuneParserByName<IEntitySet>("FUHE");

        parser.Should().BeOfType<FUHEParser>();
    }

    [Fact]
    public void Parse_WithSource_ProducesCorrectFUHE()
    {
        var mockSource = new MockEntitySet();
        ParserLookup.AddRuneParser("FUHE_HappyPath_IEntitySet", new MockParser<IEntitySet>(mockSource));

        var result = new FUHEParser().Parse(new TokenStream("FUHE_HappyPath_IEntitySet"));

        result.Succeeded.Should().BeTrue();
        var fuhe = result.Value.Should().BeOfType<FUHE>().Subject;
        fuhe.Source.Should().BeSameAs(mockSource);
    }

    [Fact]
    public void Parse_WithMissingSource_Fails()
    {
        var result = new FUHEParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeFalse();
    }
}
