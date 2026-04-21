using FluentAssertions;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.SetOperationRunes;
using RunicMagic.Tests.RuneParsing;
using RunicMagic.World.Runes.RuneTypes;
using RunicMagic.World.Runes.SetOperationRunes;
using Xunit;

namespace RunicMagic.Tests.RuneParsing.SetOperationRunes;

public class RALParserTests
{
    [Fact]
    public void ResolvesFromParserLookup()
    {
        var parser = ParserLookup.FindRuneParserByName<IEntitySet>("RAL");

        parser.Should().BeOfType<RALParser>();
    }

    [Fact]
    public void Parse_WithTwoEntitySets_ProducesCorrectRAL()
    {
        var mockLeft = new MockEntitySet();
        var mockRight = new MockEntitySet();
        ParserLookup.AddRuneParser("RAL_HappyPath_Left", new MockParser<IEntitySet>(mockLeft));
        ParserLookup.AddRuneParser("RAL_HappyPath_Right", new MockParser<IEntitySet>(mockRight));

        var result = new RALParser().Parse(new TokenStream("RAL_HappyPath_Left RAL_HappyPath_Right"));

        result.Succeeded.Should().BeTrue();
        var ral = result.Value.Should().BeOfType<RAL>().Subject;
        ral.Left.Should().BeSameAs(mockLeft);
        ral.Right.Should().BeSameAs(mockRight);
    }

    [Fact]
    public void Parse_WithMissingLeft_Fails()
    {
        var result = new RALParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public void Parse_WithMissingRight_Fails()
    {
        var mockLeft = new MockEntitySet();
        ParserLookup.AddRuneParser("RAL_MissingRight_Left", new MockParser<IEntitySet>(mockLeft));

        var result = new RALParser().Parse(new TokenStream("RAL_MissingRight_Left"));

        result.Succeeded.Should().BeFalse();
    }
}
