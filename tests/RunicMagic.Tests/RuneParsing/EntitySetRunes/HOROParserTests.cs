using FluentAssertions;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.EntitySetRunes;
using RunicMagic.Tests.RuneParsing;
using RunicMagic.World.Runes.EntityReferenceRunes;
using RunicMagic.World.Runes.EntitySetRunes;
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
    public void Parse_WithHowFarAndExplicitOrigin_ProducesCorrectHORO()
    {
        var mockHowFar = new MockNumber();
        var mockOrigin = new MockEntitySet();
        ParserLookup.AddRuneParser("HORO_WithExplicitOrigin_INumber", new MockParser<INumber>(mockHowFar));
        ParserLookup.AddRuneParser("HORO_WithExplicitOrigin_IEntitySet", new MockParser<IEntitySet>(mockOrigin));

        var result = new HOROParser().Parse(new TokenStream("HORO_WithExplicitOrigin_INumber HORO_WithExplicitOrigin_IEntitySet"));

        result.Succeeded.Should().BeTrue();
        var horo = result.Value.Should().BeOfType<HORO>().Subject;
        horo.HowFar.Should().BeSameAs(mockHowFar);
        horo.Origin.Should().BeSameAs(mockOrigin);
    }

    [Fact]
    public void Parse_WithHowFarOnly_UsesOHAsDefaultOrigin()
    {
        var mockHowFar = new MockNumber();
        ParserLookup.AddRuneParser("HORO_DefaultOrigin_INumber", new MockParser<INumber>(mockHowFar));

        var result = new HOROParser().Parse(new TokenStream("HORO_DefaultOrigin_INumber"));

        result.Succeeded.Should().BeTrue();
        var horo = result.Value.Should().BeOfType<HORO>().Subject;
        horo.HowFar.Should().BeSameAs(mockHowFar);
        horo.Origin.Should().BeOfType<OH>();
    }

    [Fact]
    public void Parse_WithMissingHowFar_Fails()
    {
        var result = new HOROParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeFalse();
    }
}
