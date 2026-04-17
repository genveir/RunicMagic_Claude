using FluentAssertions;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.ExecutionRunes;
using RunicMagic.Tests.RuneParsing;
using RunicMagic.World.Runes.ExecutionRunes;
using RunicMagic.World.Runes.RuneTypes;
using Xunit;

namespace RunicMagic.Tests.RuneParsing.ExecutionRunes;

public class ZUParserTests
{
    [Fact]
    public void ResolvesFromParserLookup()
    {
        var parser = ParserLookup.FindRuneParserByName<IExecutableStatement>("ZU");

        parser.Should().BeOfType<ZUParser>();
    }

    [Fact]
    public void Parse_WithStatement_WrapsInZU()
    {
        var mockStatement = new MockStatement();
        ParserLookup.AddRuneParser("ZU_HappyPath_IStatement", new MockParser<IStatement>(mockStatement));

        var result = new ZUParser().Parse(new TokenStream("ZU_HappyPath_IStatement"));

        result.Succeeded.Should().BeTrue();
        var zu = result.Value.Should().BeOfType<ZU>().Subject;
        zu.Statement.Should().BeSameAs(mockStatement);
    }

    [Fact]
    public void Parse_WithEmptyStream_Fails()
    {
        var result = new ZUParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeFalse();
    }
}
