using FluentAssertions;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.FilterRunes;
using RunicMagic.Tests.RuneParsing;
using RunicMagic.World.Runes.EntityReferenceRunes;
using RunicMagic.World.Runes.FilterRunes;
using RunicMagic.World.Runes.RuneTypes;
using Xunit;

namespace RunicMagic.Tests.RuneParsing.FilterRunes;

public class HORILParserTests
{
    [Fact]
    public void ResolvesFromParserLookup()
    {
        var parser = ParserLookup.FindRuneParserByName<IEntitySet>("HORIL");

        parser.Should().BeOfType<HORILParser>();
    }

    [Fact]
    public void Parse_WithAllArgsExplicit_ProducesCorrectHORIL()
    {
        var mockSource = new MockEntitySet();
        var mockLower = new MockNumber();
        var mockUpper = new MockNumber();
        var mockOrigin = new MockEntitySet();
        ParserLookup.AddRuneParser("HORIL_WithExplicitOrigin_Source_IEntitySet", new MockParser<IEntitySet>(mockSource));
        ParserLookup.AddRuneParser("HORIL_WithExplicitOrigin_Lower_INumber", new MockParser<INumber>(mockLower));
        ParserLookup.AddRuneParser("HORIL_WithExplicitOrigin_Upper_INumber", new MockParser<INumber>(mockUpper));
        ParserLookup.AddRuneParser("HORIL_WithExplicitOrigin_Origin_IEntitySet", new MockParser<IEntitySet>(mockOrigin));

        var result = new HORILParser().Parse(new TokenStream("HORIL_WithExplicitOrigin_Source_IEntitySet HORIL_WithExplicitOrigin_Lower_INumber HORIL_WithExplicitOrigin_Upper_INumber HORIL_WithExplicitOrigin_Origin_IEntitySet"));

        result.Succeeded.Should().BeTrue();
        var horil = result.Value.Should().BeOfType<HORIL>().Subject;
        horil.Source.Should().BeSameAs(mockSource);
        horil.Lower.Should().BeSameAs(mockLower);
        horil.Upper.Should().BeSameAs(mockUpper);
        horil.Origin.Should().BeSameAs(mockOrigin);
    }

    [Fact]
    public void Parse_WithSourceLowerUpperOnly_UsesOHAsDefaultOrigin()
    {
        var mockSource = new MockEntitySet();
        var mockLower = new MockNumber();
        var mockUpper = new MockNumber();
        ParserLookup.AddRuneParser("HORIL_DefaultOrigin_Source_IEntitySet", new MockParser<IEntitySet>(mockSource));
        ParserLookup.AddRuneParser("HORIL_DefaultOrigin_Lower_INumber", new MockParser<INumber>(mockLower));
        ParserLookup.AddRuneParser("HORIL_DefaultOrigin_Upper_INumber", new MockParser<INumber>(mockUpper));

        var result = new HORILParser().Parse(new TokenStream("HORIL_DefaultOrigin_Source_IEntitySet HORIL_DefaultOrigin_Lower_INumber HORIL_DefaultOrigin_Upper_INumber"));

        result.Succeeded.Should().BeTrue();
        var horil = result.Value.Should().BeOfType<HORIL>().Subject;
        horil.Source.Should().BeSameAs(mockSource);
        horil.Lower.Should().BeSameAs(mockLower);
        horil.Upper.Should().BeSameAs(mockUpper);
        horil.Origin.Should().BeOfType<OH>();
    }

    [Fact]
    public void Parse_WithMissingSource_Fails()
    {
        var result = new HORILParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public void Parse_WithMissingLower_Fails()
    {
        var mockSource = new MockEntitySet();
        ParserLookup.AddRuneParser("HORIL_MissingLower_IEntitySet", new MockParser<IEntitySet>(mockSource));

        var result = new HORILParser().Parse(new TokenStream("HORIL_MissingLower_IEntitySet"));

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public void Parse_WithMissingUpper_Fails()
    {
        var mockSource = new MockEntitySet();
        var mockLower = new MockNumber();
        ParserLookup.AddRuneParser("HORIL_MissingUpper_IEntitySet", new MockParser<IEntitySet>(mockSource));
        ParserLookup.AddRuneParser("HORIL_MissingUpper_INumber", new MockParser<INumber>(mockLower));

        var result = new HORILParser().Parse(new TokenStream("HORIL_MissingUpper_IEntitySet HORIL_MissingUpper_INumber"));

        result.Succeeded.Should().BeFalse();
    }
}
