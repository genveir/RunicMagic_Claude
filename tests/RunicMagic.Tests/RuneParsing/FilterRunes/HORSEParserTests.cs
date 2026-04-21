using FluentAssertions;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.FilterRunes;
using RunicMagic.Tests.RuneParsing;
using RunicMagic.World.Runes.EntityReferenceRunes;
using RunicMagic.World.Runes.FilterRunes;
using RunicMagic.World.Runes.RuneTypes;
using Xunit;

namespace RunicMagic.Tests.RuneParsing.FilterRunes;

public class HORSEParserTests
{
    [Fact]
    public void ResolvesFromParserLookup()
    {
        var parser = ParserLookup.FindRuneParserByName<IEntitySet>("HORSE");

        parser.Should().BeOfType<HORSEParser>();
    }

    [Fact]
    public void Parse_WithSourceAndExplicitOrigin_ProducesCorrectHORSE()
    {
        var mockSource = new MockEntitySet();
        var mockOrigin = new MockEntitySet();
        ParserLookup.AddRuneParser("HORSE_WithExplicitOrigin_Source_IEntitySet", new MockParser<IEntitySet>(mockSource));
        ParserLookup.AddRuneParser("HORSE_WithExplicitOrigin_Origin_IEntitySet", new MockParser<IEntitySet>(mockOrigin));

        var result = new HORSEParser().Parse(new TokenStream("HORSE_WithExplicitOrigin_Source_IEntitySet HORSE_WithExplicitOrigin_Origin_IEntitySet"));

        result.Succeeded.Should().BeTrue();
        var horse = result.Value.Should().BeOfType<HORSE>().Subject;
        horse.Source.Should().BeSameAs(mockSource);
        horse.Origin.Should().BeSameAs(mockOrigin);
    }

    [Fact]
    public void Parse_WithSourceOnly_UsesOHAsDefaultOrigin()
    {
        var mockSource = new MockEntitySet();
        ParserLookup.AddRuneParser("HORSE_DefaultOrigin_IEntitySet", new MockParser<IEntitySet>(mockSource));

        var result = new HORSEParser().Parse(new TokenStream("HORSE_DefaultOrigin_IEntitySet"));

        result.Succeeded.Should().BeTrue();
        var horse = result.Value.Should().BeOfType<HORSE>().Subject;
        horse.Source.Should().BeSameAs(mockSource);
        horse.Origin.Should().BeOfType<OH>();
    }

    [Fact]
    public void Parse_WithMissingSource_Fails()
    {
        var result = new HORSEParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeFalse();
    }
}
