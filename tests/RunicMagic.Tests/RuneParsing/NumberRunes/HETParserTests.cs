using FluentAssertions;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.NumberRunes;
using RunicMagic.World.Runes.NumberRunes;
using RunicMagic.World.Runes.RuneTypes;
using Xunit;

namespace RunicMagic.Tests.RuneParsing.NumberRunes;

public class HETParserTests
{
    [Fact]
    public void ResolvesFromParserLookup()
    {
        var parser = ParserLookup.FindRuneParserByName<INumber>("HET");

        parser.Should().BeOfType<HETParser>();
    }

    [Fact]
    public void Parse_ReturnsHET()
    {
        var result = new HETParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeTrue();
        result.Value.Should().BeOfType<HET>();
    }
}
