using FluentAssertions;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.FilterRunes;
using RunicMagic.Tests.RuneParsing;
using RunicMagic.World.Runes.FilterRunes;
using RunicMagic.World.Runes.RuneTypes;
using Xunit;

namespace RunicMagic.Tests.RuneParsing.FilterRunes;

public class FUSEParserTests
{
    [Fact]
    public void ResolvesFromParserLookup()
    {
        var parser = ParserLookup.FindRuneParserByName<IEntitySet>("FUSE");

        parser.Should().BeOfType<FUSEParser>();
    }

    [Fact]
    public void Parse_WithSource_ProducesCorrectFUSE()
    {
        var mockSource = new MockEntitySet();
        ParserLookup.AddRuneParser("FUSE_HappyPath_IEntitySet", new MockParser<IEntitySet>(mockSource));

        var result = new FUSEParser().Parse(new TokenStream("FUSE_HappyPath_IEntitySet"));

        result.Succeeded.Should().BeTrue();
        var fuse = result.Value.Should().BeOfType<FUSE>().Subject;
        fuse.Source.Should().BeSameAs(mockSource);
    }

    [Fact]
    public void Parse_WithMissingSource_Fails()
    {
        var result = new FUSEParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeFalse();
    }
}
