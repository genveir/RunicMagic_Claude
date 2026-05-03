using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.DecoratorRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Tests.Controller.RuneParsing.DecoratorRunes;

public class SAParserTests
{
    [Fact]
    public void ResolvesFromParserLookup_ForEntitySet()
    {
        var parser = ParserLookup.FindRuneParserByName<IEntitySet>("SA");

        parser.Should().BeOfType<SAParser<IEntitySet>>();
    }

    [Fact]
    public void ResolvesFromParserLookup_ForNumber()
    {
        var parser = ParserLookup.FindRuneParserByName<INumber>("SA");

        parser.Should().BeOfType<SAParser<INumber>>();
    }

    [Fact]
    public void ResolvesFromParserLookup_ForLocation()
    {
        var parser = ParserLookup.FindRuneParserByName<ILocation>("SA");

        parser.Should().BeOfType<SAParser<ILocation>>();
    }

    [Fact]
    public void Parse_EntitySet_InnerValueNotWrappedInCalcified()
    {
        // SA switches to Live mode, so the inner entity set is not wrapped in CalcifiedEntitySet.
        var mockEntitySet = new MockEntitySet();
        ParserLookup.AddRuneParser("SA_EntitySet_Test", new MockParser<IEntitySet>(mockEntitySet));

        var result = RuneParsingDispatcher.ParseNextRune<IEntitySet>(new TokenStream("SA SA_EntitySet_Test"));

        result.Succeeded.Should().BeTrue();
        result.Value.Should().BeSameAs(mockEntitySet);
    }

    [Fact]
    public void Parse_Number_InnerValueNotWrappedInCalcified()
    {
        var mockNumber = new MockNumber();
        ParserLookup.AddRuneParser("SA_Number_Test", new MockParser<INumber>(mockNumber));

        var result = RuneParsingDispatcher.ParseNextRune<INumber>(new TokenStream("SA SA_Number_Test"));

        result.Succeeded.Should().BeTrue();
        result.Value.Should().BeSameAs(mockNumber);
    }

    [Fact]
    public void Parse_Location_InnerValueNotWrappedInCalcified()
    {
        var mockLocation = new MockLocation();
        ParserLookup.AddRuneParser("SA_Location_Test", new MockParser<ILocation>(mockLocation));

        var result = RuneParsingDispatcher.ParseNextRune<ILocation>(new TokenStream("SA SA_Location_Test"));

        result.Succeeded.Should().BeTrue();
        result.Value.Should().BeSameAs(mockLocation);
    }

    [Fact]
    public void Parse_RestoresModeAfterParsing()
    {
        var mockEntitySet = new MockEntitySet();
        ParserLookup.AddRuneParser("SA_ModeRestore_Test", new MockParser<IEntitySet>(mockEntitySet));
        var tokenStream = new TokenStream("SA SA_ModeRestore_Test");

        RuneParsingDispatcher.ParseNextRune<IEntitySet>(tokenStream);

        tokenStream.LivenessMode.Should().Be(LivenessMode.Calcified);
    }

    [Fact]
    public void Parse_ResultIsLive()
    {
        var mockEntitySet = new MockEntitySet();
        ParserLookup.AddRuneParser("SA_Live_Test", new MockParser<IEntitySet>(mockEntitySet));

        var result = RuneParsingDispatcher.ParseNextRune<IEntitySet>(new TokenStream("SA SA_Live_Test"));

        result.IsLive.Should().BeTrue();
    }

    [Fact]
    public void Parse_WithFailingInner_Fails()
    {
        var result = RuneParsingDispatcher.ParseNextRune<IEntitySet>(new TokenStream("SA"));

        result.Succeeded.Should().BeFalse();
    }
}
