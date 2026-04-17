using FluentAssertions;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.World.Runes.EntityReferenceRunes;
using RunicMagic.World.Runes.RuneTypes;
using Xunit;

namespace RunicMagic.Tests.RuneParsing;

public class RuneParsingDispatcherTests
{
    [Fact]
    public void ParseNextRune_WithRegisteredToken_ReturnsValueFromParser()
    {
        var mockEntitySet = new MockEntitySet();
        ParserLookup.AddRuneParser("Dispatcher_HappyPath_IEntitySet", new MockParser<IEntitySet>(mockEntitySet));

        var result = RuneParsingDispatcher.ParseNextRune<IEntitySet>(new TokenStream("Dispatcher_HappyPath_IEntitySet"));

        result.Succeeded.Should().BeTrue();
        result.Value.Should().BeSameAs(mockEntitySet);
    }

    [Fact]
    public void ParseNextRune_WithEmptyStream_Fails()
    {
        var result = RuneParsingDispatcher.ParseNextRune<IEntitySet>(new TokenStream(""));

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public void ParseNextRune_WithUnrecognizedToken_Fails()
    {
        var result = RuneParsingDispatcher.ParseNextRune<IEntitySet>(new TokenStream("Dispatcher_Unrecognized_IEntitySet"));

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public void ParseNextRune_WhenParserFails_PropagatesFailure()
    {
        ParserLookup.AddRuneParser("Dispatcher_FailingParser_IEntitySet", new MockFailingParser<IEntitySet>("test error"));

        var result = RuneParsingDispatcher.ParseNextRune<IEntitySet>(new TokenStream("Dispatcher_FailingParser_IEntitySet"));

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public void ParseNextRune_WithDefaultsAndEmptyDefaultsArray_Throws()
    {
        var act = () => RuneParsingDispatcher.ParseNextRune<IEntitySet>(new TokenStream(""), defaultTokens: []);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ParseNextRune_WithDefaultsAndRegisteredToken_ReturnsValueFromParser()
    {
        var mockEntitySet = new MockEntitySet();
        ParserLookup.AddRuneParser("Dispatcher_HappyPathWithDefaults_IEntitySet", new MockParser<IEntitySet>(mockEntitySet));

        var result = RuneParsingDispatcher.ParseNextRune<IEntitySet>(new TokenStream("Dispatcher_HappyPathWithDefaults_IEntitySet"), defaultTokens: ["A"]);

        result.Succeeded.Should().BeTrue();
        result.Value.Should().BeSameAs(mockEntitySet);
    }

    [Fact]
    public void ParseNextRune_WithDefaultsAndEmptyStream_UsesDefaults()
    {
        var result = RuneParsingDispatcher.ParseNextRune<IEntitySet>(new TokenStream(""), defaultTokens: ["A"]);

        result.Succeeded.Should().BeTrue();
        result.Value.Should().BeOfType<A>();
    }

    [Fact]
    public void ParseNextRune_WithDefaultsAndUnrecognizedToken_UsesDefaults()
    {
        var result = RuneParsingDispatcher.ParseNextRune<IEntitySet>(new TokenStream("Dispatcher_Unrecognized_IEntitySet"), defaultTokens: ["A"]);

        result.Succeeded.Should().BeTrue();
        result.Value.Should().BeOfType<A>();
    }
}
