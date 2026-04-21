using FluentAssertions;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.FilterRunes;
using RunicMagic.Tests.RuneParsing;
using RunicMagic.World.Runes.FilterRunes;
using RunicMagic.World.Runes.RuneTypes;
using Xunit;

namespace RunicMagic.Tests.RuneParsing.FilterRunes;

public class FUILParserTests
{
    [Fact]
    public void ResolvesFromParserLookup()
    {
        var parser = ParserLookup.FindRuneParserByName<IEntitySet>("FUIL");

        parser.Should().BeOfType<FUILParser>();
    }

    [Fact]
    public void Parse_WithAllArgs_ProducesCorrectFUIL()
    {
        var mockSource = new MockEntitySet();
        var mockLower = new MockNumber();
        var mockUpper = new MockNumber();
        ParserLookup.AddRuneParser("FUIL_HappyPath_IEntitySet", new MockParser<IEntitySet>(mockSource));
        ParserLookup.AddRuneParser("FUIL_HappyPath_Lower_INumber", new MockParser<INumber>(mockLower));
        ParserLookup.AddRuneParser("FUIL_HappyPath_Upper_INumber", new MockParser<INumber>(mockUpper));

        var result = new FUILParser().Parse(new TokenStream("FUIL_HappyPath_IEntitySet FUIL_HappyPath_Lower_INumber FUIL_HappyPath_Upper_INumber"));

        result.Succeeded.Should().BeTrue();
        var fuil = result.Value.Should().BeOfType<FUIL>().Subject;
        fuil.Source.Should().BeSameAs(mockSource);
        fuil.Lower.Should().BeSameAs(mockLower);
        fuil.Upper.Should().BeSameAs(mockUpper);
    }

    [Fact]
    public void Parse_WithMissingSource_Fails()
    {
        var result = new FUILParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public void Parse_WithMissingLower_Fails()
    {
        var mockSource = new MockEntitySet();
        ParserLookup.AddRuneParser("FUIL_MissingLower_IEntitySet", new MockParser<IEntitySet>(mockSource));

        var result = new FUILParser().Parse(new TokenStream("FUIL_MissingLower_IEntitySet"));

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public void Parse_WithMissingUpper_Fails()
    {
        var mockSource = new MockEntitySet();
        var mockLower = new MockNumber();
        ParserLookup.AddRuneParser("FUIL_MissingUpper_IEntitySet", new MockParser<IEntitySet>(mockSource));
        ParserLookup.AddRuneParser("FUIL_MissingUpper_INumber", new MockParser<INumber>(mockLower));

        var result = new FUILParser().Parse(new TokenStream("FUIL_MissingUpper_IEntitySet FUIL_MissingUpper_INumber"));

        result.Succeeded.Should().BeFalse();
    }
}
