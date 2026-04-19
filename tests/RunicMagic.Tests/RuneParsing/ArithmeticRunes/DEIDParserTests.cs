using FluentAssertions;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.ArithmeticRunes;
using RunicMagic.World.Runes.NumberRunes;
using RunicMagic.World.Runes.RuneTypes;
using Xunit;

namespace RunicMagic.Tests.RuneParsing.ArithmeticRunes;

public class DEIDParserTests
{
    [Fact]
    public void ResolvesFromParserLookup()
    {
        var parser = ParserLookup.FindRuneParserByName<INumber>("DEID");

        parser.Should().BeOfType<DEIDParser>();
    }

    [Fact]
    public void Parse_WithNumber_WrapsInDEID()
    {
        var mockA = new MockNumber();
        ParserLookup.AddRuneParser("DEID_A_INumber", new MockParser<INumber>(mockA));

        var result = new DEIDParser().Parse(new TokenStream("DEID_A_INumber"));

        result.Succeeded.Should().BeTrue();
        var deid = result.Value.Should().BeOfType<DEID>().Subject;
        deid.A.Should().BeSameAs(mockA);
    }

    [Fact]
    public void Parse_WithEmptyStream_Fails()
    {
        var result = new DEIDParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeFalse();
    }
}
