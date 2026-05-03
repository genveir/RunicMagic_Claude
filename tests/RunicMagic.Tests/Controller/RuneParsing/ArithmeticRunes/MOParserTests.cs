using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.ArithmeticRunes;
using RunicMagic.World.Runes.NumberRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Tests.Controller.RuneParsing.ArithmeticRunes;

public class MOParserTests
{
    [Fact]
    public void ResolvesFromParserLookup()
    {
        var parser = ParserLookup.FindRuneParserByName<INumber>("MO");

        parser.Should().BeOfType<MOParser>();
    }

    [Fact]
    public void Parse_WithTwoNumbers_WrapsInMO()
    {
        var mockA = new MockNumber();
        var mockB = new MockNumber();
        ParserLookup.AddRuneParser("MO_A_INumber", new MockParser<INumber>(mockA));
        ParserLookup.AddRuneParser("MO_B_INumber", new MockParser<INumber>(mockB));

        var result = new MOParser().Parse(new TokenStream("MO_A_INumber MO_B_INumber"));

        result.Succeeded.Should().BeTrue();
        var mo = result.Value.Should().BeOfType<MO>().Subject;
        mo.A.Should().BeOfType<CalcifiedNumber>().Which.Inner.Should().BeSameAs(mockA);
        mo.B.Should().BeOfType<CalcifiedNumber>().Which.Inner.Should().BeSameAs(mockB);
    }

    [Fact]
    public void Parse_WithEmptyStream_Fails()
    {
        var result = new MOParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public void Parse_WithOnlyOneNumber_Fails()
    {
        var mockA = new MockNumber();
        ParserLookup.AddRuneParser("MO_OnlyOne_INumber", new MockParser<INumber>(mockA));

        var result = new MOParser().Parse(new TokenStream("MO_OnlyOne_INumber"));

        result.Succeeded.Should().BeFalse();
    }
}
