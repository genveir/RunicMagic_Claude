using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.EntitySetRunes;
using RunicMagic.World.Runes.EntitySetRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Tests.Controller.RuneParsing.EntitySetRunes;

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
