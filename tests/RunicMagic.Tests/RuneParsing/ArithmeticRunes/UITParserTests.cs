using FluentAssertions;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.ArithmeticRunes;
using RunicMagic.World.Runes.NumberRunes;
using RunicMagic.World.Runes.RuneTypes;
using Xunit;

namespace RunicMagic.Tests.RuneParsing.ArithmeticRunes;

public class UITParserTests
{
    [Fact]
    public void ResolvesFromParserLookup()
    {
        var parser = ParserLookup.FindRuneParserByName<INumber>("UIT");

        parser.Should().BeOfType<UITParser>();
    }

    [Fact]
    public void Parse_WithTwoNumbers_WrapsInUIT()
    {
        var mockA = new MockNumber();
        var mockB = new MockNumber();
        ParserLookup.AddRuneParser("UIT_A_INumber", new MockParser<INumber>(mockA));
        ParserLookup.AddRuneParser("UIT_B_INumber", new MockParser<INumber>(mockB));

        var result = new UITParser().Parse(new TokenStream("UIT_A_INumber UIT_B_INumber"));

        result.Succeeded.Should().BeTrue();
        var uit = result.Value.Should().BeOfType<UIT>().Subject;
        uit.A.Should().BeSameAs(mockA);
        uit.B.Should().BeSameAs(mockB);
    }

    [Fact]
    public void Parse_WithEmptyStream_Fails()
    {
        var result = new UITParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public void Parse_WithOnlyOneNumber_Fails()
    {
        var mockA = new MockNumber();
        ParserLookup.AddRuneParser("UIT_OnlyOne_INumber", new MockParser<INumber>(mockA));

        var result = new UITParser().Parse(new TokenStream("UIT_OnlyOne_INumber"));

        result.Succeeded.Should().BeFalse();
    }
}
