using FluentAssertions;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.ArithmeticRunes;
using RunicMagic.World.Runes.NumberRunes;
using RunicMagic.World.Runes.RuneTypes;
using Xunit;

namespace RunicMagic.Tests.RuneParsing.ArithmeticRunes;

public class IRParserTests
{
    [Fact]
    public void ResolvesFromParserLookup()
    {
        var parser = ParserLookup.FindRuneParserByName<INumber>("IR");

        parser.Should().BeOfType<IRParser>();
    }

    [Fact]
    public void Parse_WithTwoNumbers_WrapsInIR()
    {
        var mockA = new MockNumber();
        var mockB = new MockNumber();
        ParserLookup.AddRuneParser("IR_A_INumber", new MockParser<INumber>(mockA));
        ParserLookup.AddRuneParser("IR_B_INumber", new MockParser<INumber>(mockB));

        var result = new IRParser().Parse(new TokenStream("IR_A_INumber IR_B_INumber"));

        result.Succeeded.Should().BeTrue();
        var ir = result.Value.Should().BeOfType<IR>().Subject;
        ir.A.Should().BeSameAs(mockA);
        ir.B.Should().BeSameAs(mockB);
    }

    [Fact]
    public void Parse_WithEmptyStream_Fails()
    {
        var result = new IRParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public void Parse_WithOnlyOneNumber_Fails()
    {
        var mockA = new MockNumber();
        ParserLookup.AddRuneParser("IR_OnlyOne_INumber", new MockParser<INumber>(mockA));

        var result = new IRParser().Parse(new TokenStream("IR_OnlyOne_INumber"));

        result.Succeeded.Should().BeFalse();
    }
}
