using FluentAssertions;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.DebugRunes;
using RunicMagic.World.Runes.DebugRunes;
using RunicMagic.World.Runes.RuneTypes;
using Xunit;

namespace RunicMagic.Tests.RuneParsing.DebugRunes;

public class DETAILSParserTests
{
    [Fact]
    public void ResolvesFromParserLookup()
    {
        var parser = ParserLookup.FindRuneParserByName<IStatement>("DETAILS");

        parser.Should().BeOfType<DETAILSParser>();
    }

    [Fact]
    public void Parse_WithEntitySet_ProducesDETAILS()
    {
        var mockEntitySet = new MockEntitySet();
        ParserLookup.AddRuneParser("DETAILS_HappyPath_IEntitySet", new MockParser<IEntitySet>(mockEntitySet));

        var result = new DETAILSParser().Parse(new TokenStream("DETAILS_HappyPath_IEntitySet"));

        result.Succeeded.Should().BeTrue();
        result.Value.Should().BeOfType<DETAILS>();
    }

    [Fact]
    public void Parse_WithEmptyTokenStream_DefaultsToDAN()
    {
        var result = new DETAILSParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeTrue();
        result.Value.Should().BeOfType<DETAILS>();
    }
}
