using FluentAssertions;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.PowerSourceRunes;
using RunicMagic.World.Runes.PowerSourceRunes;
using RunicMagic.World.Runes.RuneTypes;
using Xunit;

namespace RunicMagic.Tests.RuneParsing.PowerSourceRunes;

public class SHUParserTests
{
    [Fact]
    public void ResolvesFromParserLookup()
    {
        var parser = ParserLookup.FindRuneParserByName<IStatement>("SHU");

        parser.Should().BeOfType<SHUParser>();
    }

    [Fact]
    public void Parse_HappyPath_ProducesCorrectSHU()
    {
        var mockSource = new MockEntitySet();
        var mockStatement = new MockStatement();
        ParserLookup.AddRuneParser("SHU_HappyPath_IEntitySet", new MockParser<IEntitySet>(mockSource));
        ParserLookup.AddRuneParser("SHU_HappyPath_IStatement", new MockParser<IStatement>(mockStatement));

        var result = new SHUParser().Parse(new TokenStream("SHU_HappyPath_IEntitySet SHU_HappyPath_IStatement"));

        result.Succeeded.Should().BeTrue();
        var shu = result.Value.Should().BeOfType<SHU>().Subject;
        shu.Source.Should().BeSameAs(mockSource);
        shu.Statement.Should().BeSameAs(mockStatement);
    }

    [Fact]
    public void Parse_MissingEntitySet_Fails()
    {
        var result = new SHUParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public void Parse_MissingStatement_Fails()
    {
        var mockSource = new MockEntitySet();
        ParserLookup.AddRuneParser("SHU_MissingStatement_IEntitySet", new MockParser<IEntitySet>(mockSource));

        var result = new SHUParser().Parse(new TokenStream("SHU_MissingStatement_IEntitySet"));

        result.Succeeded.Should().BeFalse();
    }
}
