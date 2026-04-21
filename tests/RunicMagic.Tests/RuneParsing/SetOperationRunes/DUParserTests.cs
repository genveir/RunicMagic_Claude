using FluentAssertions;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.SetOperationRunes;
using RunicMagic.Tests.RuneParsing;
using RunicMagic.World.Runes.RuneTypes;
using RunicMagic.World.Runes.SetOperationRunes;
using Xunit;

namespace RunicMagic.Tests.RuneParsing.SetOperationRunes;

public class DUParserTests
{
    [Fact]
    public void ResolvesFromParserLookup()
    {
        var parser = ParserLookup.FindRuneParserByName<IEntitySet>("DU");

        parser.Should().BeOfType<DUParser>();
    }

    [Fact]
    public void Parse_WithTwoEntitySets_ProducesCorrectDU()
    {
        var mockLeft = new MockEntitySet();
        var mockRight = new MockEntitySet();
        ParserLookup.AddRuneParser("DU_HappyPath_Left", new MockParser<IEntitySet>(mockLeft));
        ParserLookup.AddRuneParser("DU_HappyPath_Right", new MockParser<IEntitySet>(mockRight));

        var result = new DUParser().Parse(new TokenStream("DU_HappyPath_Left DU_HappyPath_Right"));

        result.Succeeded.Should().BeTrue();
        var du = result.Value.Should().BeOfType<DU>().Subject;
        du.Left.Should().BeSameAs(mockLeft);
        du.Right.Should().BeSameAs(mockRight);
    }

    [Fact]
    public void Parse_WithMissingLeft_Fails()
    {
        var result = new DUParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public void Parse_WithMissingRight_Fails()
    {
        var mockLeft = new MockEntitySet();
        ParserLookup.AddRuneParser("DU_MissingRight_Left", new MockParser<IEntitySet>(mockLeft));

        var result = new DUParser().Parse(new TokenStream("DU_MissingRight_Left"));

        result.Succeeded.Should().BeFalse();
    }
}
