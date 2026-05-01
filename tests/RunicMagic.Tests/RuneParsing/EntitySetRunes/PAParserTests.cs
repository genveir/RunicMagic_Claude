using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.EntitySetRunes;
using RunicMagic.World.Runes.EntityReferenceRunes;
using RunicMagic.World.Runes.EntitySetRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Tests.RuneParsing.EntitySetRunes;

public class PAParserTests
{
    [Fact]
    public void ResolvesFromParserLookup()
    {
        var parser = ParserLookup.FindRuneParserByName<IEntitySet>("PA");

        parser.Should().BeOfType<PAParser>();
    }

    [Fact]
    public void Parse_WithExplicitEntitySet_WrapsInPA()
    {
        var mockEntitySet = new MockEntitySet();
        ParserLookup.AddRuneParser("PA_HappyPath_IEntitySet", new MockParser<IEntitySet>(mockEntitySet));

        var result = new PAParser().Parse(new TokenStream("PA_HappyPath_IEntitySet"));

        result.Succeeded.Should().BeTrue();
        var pa = result.Value.Should().BeOfType<PA>().Subject;
        pa.ToGetScopeOf.Should().BeOfType<CalcifiedEntitySet>().Which.Inner.Should().BeSameAs(mockEntitySet);
    }

    [Fact]
    public void Parse_WithDefaultEntitySet_UsesOH()
    {
        var result = new PAParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeTrue();
        var pa = result.Value.Should().BeOfType<PA>().Subject;
        pa.ToGetScopeOf.Should().BeOfType<CalcifiedEntitySet>().Which.Inner.Should().BeOfType<OH>();
    }
}
