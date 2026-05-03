using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.FilterRunes;
using RunicMagic.World.Runes.FilterRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Tests.Controller.RuneParsing.FilterRunes;

public class ZYILParserTests
{
    [Fact]
    public void ResolvesFromParserLookup()
    {
        var parser = ParserLookup.FindRuneParserByName<IEntitySet>("ZYIL");

        parser.Should().BeOfType<ZYILParser>();
    }

    [Fact]
    public void Parse_WithAllArgs_ProducesCorrectZYIL()
    {
        var mockSource = new MockEntitySet();
        var mockLower = new MockNumber();
        var mockUpper = new MockNumber();
        ParserLookup.AddRuneParser("ZYIL_HappyPath_IEntitySet", new MockParser<IEntitySet>(mockSource));
        ParserLookup.AddRuneParser("ZYIL_HappyPath_Lower_INumber", new MockParser<INumber>(mockLower));
        ParserLookup.AddRuneParser("ZYIL_HappyPath_Upper_INumber", new MockParser<INumber>(mockUpper));

        var result = new ZYILParser().Parse(new TokenStream("ZYIL_HappyPath_IEntitySet ZYIL_HappyPath_Lower_INumber ZYIL_HappyPath_Upper_INumber"));

        result.Succeeded.Should().BeTrue();
        var zyil = result.Value.Should().BeOfType<ZYIL>().Subject;
        zyil.Source.Should().BeOfType<CalcifiedEntitySet>().Which.Inner.Should().BeSameAs(mockSource);
        zyil.Lower.Should().BeOfType<CalcifiedNumber>().Which.Inner.Should().BeSameAs(mockLower);
        zyil.Upper.Should().BeOfType<CalcifiedNumber>().Which.Inner.Should().BeSameAs(mockUpper);
    }

    [Fact]
    public void Parse_WithMissingSource_Fails()
    {
        var result = new ZYILParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public void Parse_WithMissingLower_Fails()
    {
        var mockSource = new MockEntitySet();
        ParserLookup.AddRuneParser("ZYIL_MissingLower_IEntitySet", new MockParser<IEntitySet>(mockSource));

        var result = new ZYILParser().Parse(new TokenStream("ZYIL_MissingLower_IEntitySet"));

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public void Parse_WithMissingUpper_Fails()
    {
        var mockSource = new MockEntitySet();
        var mockLower = new MockNumber();
        ParserLookup.AddRuneParser("ZYIL_MissingUpper_IEntitySet", new MockParser<IEntitySet>(mockSource));
        ParserLookup.AddRuneParser("ZYIL_MissingUpper_INumber", new MockParser<INumber>(mockLower));

        var result = new ZYILParser().Parse(new TokenStream("ZYIL_MissingUpper_IEntitySet ZYIL_MissingUpper_INumber"));

        result.Succeeded.Should().BeFalse();
    }
}
