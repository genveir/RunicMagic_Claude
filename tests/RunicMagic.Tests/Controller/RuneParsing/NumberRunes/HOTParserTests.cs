using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.NumberRunes;
using RunicMagic.World.Runes.NumberRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Tests.Controller.RuneParsing.NumberRunes;

public class HOTParserTests
{
    [Fact]
    public void ResolvesFromParserLookup()
    {
        var parser = ParserLookup.FindRuneParserByName<INumber>("HOT");

        parser.Should().BeOfType<HOTParser>();
    }

    [Fact]
    public void Parse_ReturnsHOT()
    {
        var result = new HOTParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeTrue();
        result.Value.Should().BeOfType<HOT>();
    }
}
