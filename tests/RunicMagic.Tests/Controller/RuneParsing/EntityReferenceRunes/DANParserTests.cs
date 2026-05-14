using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.EntityReferenceRunes;
using RunicMagic.World.Runes.EntityReferenceRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Tests.Controller.RuneParsing.EntityReferenceRunes;

public class DANParserTests
{
    [Fact]
    public void ResolvesFromParserLookup()
    {
        var parser = ParserLookup.FindRuneParserByName<IEntitySet>("DAN");

        parser.Should().BeOfType<DANParser>();
    }

    [Fact]
    public void Parse_ReturnsDAN()
    {
        var result = new DANParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeTrue();
        result.Value.Should().BeOfType<DAN>();
    }
}
