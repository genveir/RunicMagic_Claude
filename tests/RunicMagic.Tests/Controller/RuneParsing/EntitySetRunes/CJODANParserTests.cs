using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.EntitySetRunes;
using RunicMagic.World.Runes.EntitySetRunes;
using RunicMagic.World.Runes.NumberRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Tests.Controller.RuneParsing.EntitySetRunes;

public class CJODANParserTests
{
    [Fact]
    public void ResolvesFromParserLookup()
    {
        var parser = ParserLookup.FindRuneParserByName<IEntitySet>("CJODAN");

        parser.Should().BeOfType<CJODANParser>();
    }

    [Fact]
    public void Parse_WithRangeAndExplicitHalfAngle_ProducesCorrectCJODAN()
    {
        var mockRange = new MockNumber();
        var mockHalfAngle = new MockNumber();
        ParserLookup.AddRuneParser("CJODAN_Explicit_Range_INumber", new MockParser<INumber>(mockRange));
        ParserLookup.AddRuneParser("CJODAN_Explicit_HalfAngle_INumber", new MockParser<INumber>(mockHalfAngle));

        var result = new CJODANParser().Parse(new TokenStream("CJODAN_Explicit_Range_INumber CJODAN_Explicit_HalfAngle_INumber"));

        result.Succeeded.Should().BeTrue();
        var cjodan = result.Value.Should().BeOfType<CJODAN>().Subject;
        cjodan.Range.Should().BeOfType<CalcifiedNumber>().Which.Inner.Should().BeSameAs(mockRange);
        cjodan.HalfAngle.Should().BeOfType<CalcifiedNumber>().Which.Inner.Should().BeSameAs(mockHalfAngle);
    }

    [Fact]
    public void Parse_WithRangeOnly_UsesDOTAsDefaultHalfAngle()
    {
        var mockRange = new MockNumber();
        ParserLookup.AddRuneParser("CJODAN_DefaultHalfAngle_INumber", new MockParser<INumber>(mockRange));

        var result = new CJODANParser().Parse(new TokenStream("CJODAN_DefaultHalfAngle_INumber"));

        result.Succeeded.Should().BeTrue();
        var cjodan = result.Value.Should().BeOfType<CJODAN>().Subject;
        cjodan.Range.Should().BeOfType<CalcifiedNumber>().Which.Inner.Should().BeSameAs(mockRange);
        cjodan.HalfAngle.Should().BeOfType<CalcifiedNumber>().Which.Inner.Should().BeOfType<DOT>();
    }

    [Fact]
    public void Parse_WithMissingRange_Fails()
    {
        var result = new CJODANParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeFalse();
    }
}
