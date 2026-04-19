using FluentAssertions;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.ArithmeticRunes;
using RunicMagic.World.Runes.NumberRunes;
using RunicMagic.World.Runes.RuneTypes;
using Xunit;

namespace RunicMagic.Tests.RuneParsing.ArithmeticRunes;

public class EIDParserTests
{
    [Fact]
    public void ResolvesFromParserLookup()
    {
        var parser = ParserLookup.FindRuneParserByName<INumber>("EID");

        parser.Should().BeOfType<EIDParser>();
    }

    [Fact]
    public void Parse_WithTwoNumbers_WrapsInEID()
    {
        var mockA = new MockNumber();
        var mockB = new MockNumber();
        ParserLookup.AddRuneParser("EID_A_INumber", new MockParser<INumber>(mockA));
        ParserLookup.AddRuneParser("EID_B_INumber", new MockParser<INumber>(mockB));

        var result = new EIDParser().Parse(new TokenStream("EID_A_INumber EID_B_INumber"));

        result.Succeeded.Should().BeTrue();
        var eid = result.Value.Should().BeOfType<EID>().Subject;
        eid.A.Should().BeSameAs(mockA);
        eid.B.Should().BeSameAs(mockB);
    }

    [Fact]
    public void Parse_WithEmptyStream_Fails()
    {
        var result = new EIDParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public void Parse_WithOnlyOneNumber_Fails()
    {
        var mockA = new MockNumber();
        ParserLookup.AddRuneParser("EID_OnlyOne_INumber", new MockParser<INumber>(mockA));

        var result = new EIDParser().Parse(new TokenStream("EID_OnlyOne_INumber"));

        result.Succeeded.Should().BeFalse();
    }
}
