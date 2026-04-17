using FluentAssertions;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.EntitySetRunes;
using RunicMagic.Tests.RuneParsing;
using RunicMagic.World.Runes.EntityReferenceRunes;
using RunicMagic.World.Runes.EntitySetRunes;
using RunicMagic.World.Runes.RuneTypes;
using Xunit;

namespace RunicMagic.Tests.RuneParsing.EntitySetRunes;

public class LAParserTests
{
    [Fact]
    public void ResolvesFromParserLookup()
    {
        var parser = ParserLookup.FindRuneParserByName<IEntitySet>("LA");

        parser.Should().BeOfType<LAParser>();
    }

    [Fact]
    public void Parse_WithExplicitEntitySet_WrapsInLA()
    {
        var mockEntitySet = new MockEntitySet();
        ParserLookup.AddRuneParser("LA_HappyPath_IEntitySet", new MockParser<IEntitySet>(mockEntitySet));

        var result = new LAParser().Parse(new TokenStream("LA_HappyPath_IEntitySet"));

        result.Succeeded.Should().BeTrue();
        var la = result.Value.Should().BeOfType<LA>().Subject;
        la.ToGetScopeOf.Should().BeSameAs(mockEntitySet);
    }

    [Fact]
    public void Parse_WithDefaultEntitySet_UsesOH()
    {
        var result = new LAParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeTrue();
        var la = result.Value.Should().BeOfType<LA>().Subject;
        la.ToGetScopeOf.Should().BeOfType<OH>();
    }
}
