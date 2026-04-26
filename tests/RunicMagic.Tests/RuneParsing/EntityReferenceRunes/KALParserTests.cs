using FluentAssertions;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.EntityReferenceRunes;
using RunicMagic.World.Runes.EntityReferenceRunes;
using RunicMagic.World.Runes.RuneTypes;
using Xunit;

namespace RunicMagic.Tests.RuneParsing.EntityReferenceRunes;

public class KALParserTests
{
    [Fact]
    public void ResolvesFromParserLookup()
    {
        var parser = ParserLookup.FindRuneParserByName<IEntitySet>("KAL");

        parser.Should().BeOfType<KALParser>();
    }

    [Fact]
    public void Parse_ReturnsKAL()
    {
        var result = new KALParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeTrue();
        result.Value.Should().BeOfType<KAL>();
    }
}
