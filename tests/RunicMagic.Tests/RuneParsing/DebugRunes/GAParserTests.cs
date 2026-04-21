using FluentAssertions;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.DebugRunes;
using RunicMagic.World.Runes.DebugRunes;
using RunicMagic.World.Runes.RuneTypes;
using Xunit;

namespace RunicMagic.Tests.RuneParsing.DebugRunes;

public class GAParserTests
{
    [Fact]
    public void ResolvesFromParserLookup()
    {
        var parser = ParserLookup.FindRuneParserByName<IEntitySet>("GA");

        parser.Should().BeOfType<GAParser>();
    }

    [Fact]
    public void Parse_AlwaysSucceeds_ProducesGA()
    {
        var result = new GAParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeTrue();
        result.Value.Should().BeOfType<GA>();
    }
}
