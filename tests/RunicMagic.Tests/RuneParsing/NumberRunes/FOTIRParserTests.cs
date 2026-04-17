using FluentAssertions;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.NumberRunes;
using RunicMagic.Tests.RuneParsing;
using RunicMagic.World.Runes.NumberRunes;
using RunicMagic.World.Runes.RuneTypes;
using Xunit;

namespace RunicMagic.Tests.RuneParsing.NumberRunes;

public class FOTIRParserTests
{
    [Fact]
    public void ResolvesFromParserLookup()
    {
        var parser = ParserLookup.FindRuneParserByName<INumber>("FOTIR");

        parser.Should().BeOfType<FOTIRParser>();
    }

    [Fact]
    public void Parse_WithNumber_WrapsInFOTIR()
    {
        var mockNumber = new MockNumber();
        ParserLookup.AddRuneParser("FOTIR_HappyPath_INumber", new MockParser<INumber>(mockNumber));

        var result = new FOTIRParser().Parse(new TokenStream("FOTIR_HappyPath_INumber"));

        result.Succeeded.Should().BeTrue();
        var fotir = result.Value.Should().BeOfType<FOTIR>().Subject;
        fotir.Multiplicand.Should().BeSameAs(mockNumber);
    }

    [Fact]
    public void Parse_WithEmptyStream_Fails()
    {
        var result = new FOTIRParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeFalse();
    }
}
