using FluentAssertions;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.ArithmeticRunes;
using RunicMagic.World.Runes.NumberRunes;
using RunicMagic.World.Runes.RuneTypes;
using Xunit;

namespace RunicMagic.Tests.RuneParsing.ArithmeticRunes;

public class MOSTParserTests
{
    [Fact]
    public void ResolvesFromParserLookup()
    {
        var parser = ParserLookup.FindRuneParserByName<INumber>("MOST");

        parser.Should().BeOfType<MOSTParser>();
    }

    [Fact]
    public void Parse_WithNumber_WrapsInMOST()
    {
        var mockA = new MockNumber();
        ParserLookup.AddRuneParser("MOST_A_INumber", new MockParser<INumber>(mockA));

        var result = new MOSTParser().Parse(new TokenStream("MOST_A_INumber"));

        result.Succeeded.Should().BeTrue();
        var most = result.Value.Should().BeOfType<MOST>().Subject;
        most.A.Should().BeSameAs(mockA);
    }

    [Fact]
    public void Parse_WithEmptyStream_Fails()
    {
        var result = new MOSTParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeFalse();
    }
}
