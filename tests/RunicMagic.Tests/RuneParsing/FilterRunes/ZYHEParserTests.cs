using FluentAssertions;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.FilterRunes;
using RunicMagic.Tests.RuneParsing;
using RunicMagic.World.Runes.FilterRunes;
using RunicMagic.World.Runes.RuneTypes;
using Xunit;

namespace RunicMagic.Tests.RuneParsing.FilterRunes;

public class ZYHEParserTests
{
    [Fact]
    public void ResolvesFromParserLookup()
    {
        var parser = ParserLookup.FindRuneParserByName<IEntitySet>("ZYHE");

        parser.Should().BeOfType<ZYHEParser>();
    }

    [Fact]
    public void Parse_WithSource_ProducesCorrectZYHE()
    {
        var mockSource = new MockEntitySet();
        ParserLookup.AddRuneParser("ZYHE_HappyPath_IEntitySet", new MockParser<IEntitySet>(mockSource));

        var result = new ZYHEParser().Parse(new TokenStream("ZYHE_HappyPath_IEntitySet"));

        result.Succeeded.Should().BeTrue();
        var zyhe = result.Value.Should().BeOfType<ZYHE>().Subject;
        zyhe.Source.Should().BeSameAs(mockSource);
    }

    [Fact]
    public void Parse_WithMissingSource_Fails()
    {
        var result = new ZYHEParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeFalse();
    }
}
