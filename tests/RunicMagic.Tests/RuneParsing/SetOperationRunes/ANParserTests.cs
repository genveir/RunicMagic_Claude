using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.SetOperationRunes;
using RunicMagic.World.Runes.RuneTypes;
using RunicMagic.World.Runes.SetOperationRunes;

namespace RunicMagic.Tests.RuneParsing.SetOperationRunes;

public class ANParserTests
{
    [Fact]
    public void ResolvesFromParserLookup()
    {
        var parser = ParserLookup.FindRuneParserByName<IEntitySet>("AN");

        parser.Should().BeOfType<ANParser>();
    }

    [Fact]
    public void Parse_WithTwoEntitySets_ProducesCorrectAN()
    {
        var mockLeft = new MockEntitySet();
        var mockRight = new MockEntitySet();
        ParserLookup.AddRuneParser("AN_HappyPath_Left", new MockParser<IEntitySet>(mockLeft));
        ParserLookup.AddRuneParser("AN_HappyPath_Right", new MockParser<IEntitySet>(mockRight));

        var result = new ANParser().Parse(new TokenStream("AN_HappyPath_Left AN_HappyPath_Right"));

        result.Succeeded.Should().BeTrue();
        var an = result.Value.Should().BeOfType<AN>().Subject;
        an.Left.Should().BeOfType<CalcifiedEntitySet>().Which.Inner.Should().BeSameAs(mockLeft);
        an.Right.Should().BeOfType<CalcifiedEntitySet>().Which.Inner.Should().BeSameAs(mockRight);
    }

    [Fact]
    public void Parse_WithMissingLeft_Fails()
    {
        var result = new ANParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public void Parse_WithMissingRight_Fails()
    {
        var mockLeft = new MockEntitySet();
        ParserLookup.AddRuneParser("AN_MissingRight_Left", new MockParser<IEntitySet>(mockLeft));

        var result = new ANParser().Parse(new TokenStream("AN_MissingRight_Left"));

        result.Succeeded.Should().BeFalse();
    }
}
