using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.DecoratorRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Tests.Controller.RuneParsing.DecoratorRunes;

public class YIParserTests
{
    [Fact]
    public void ResolvesFromParserLookup_ForEntitySet()
    {
        var parser = ParserLookup.FindRuneParserByName<IEntitySet>("YI");

        parser.Should().BeOfType<YIParser<IEntitySet>>();
    }

    [Fact]
    public void ResolvesFromParserLookup_ForNumber()
    {
        var parser = ParserLookup.FindRuneParserByName<INumber>("YI");

        parser.Should().BeOfType<YIParser<INumber>>();
    }

    [Fact]
    public void ResolvesFromParserLookup_ForLocation()
    {
        var parser = ParserLookup.FindRuneParserByName<ILocation>("YI");

        parser.Should().BeOfType<YIParser<ILocation>>();
    }

    [Fact]
    public void Parse_EntitySet_InnerValueWrappedInCalcifiedEntitySet()
    {
        // YI forces Calcified mode, so the inner entity set is wrapped in CalcifiedEntitySet.
        var mockEntitySet = new MockEntitySet();
        ParserLookup.AddRuneParser("YI_EntitySet_Test", new MockParser<IEntitySet>(mockEntitySet));

        var result = RuneParsingDispatcher.ParseNextRune<IEntitySet>(new TokenStream("YI YI_EntitySet_Test"));

        result.Succeeded.Should().BeTrue();
        result.Value.Should().BeOfType<CalcifiedEntitySet>().Which.Inner.Should().BeSameAs(mockEntitySet);
    }

    [Fact]
    public void Parse_Number_InnerValueWrappedInCalcifiedNumber()
    {
        var mockNumber = new MockNumber();
        ParserLookup.AddRuneParser("YI_Number_Test", new MockParser<INumber>(mockNumber));

        var result = RuneParsingDispatcher.ParseNextRune<INumber>(new TokenStream("YI YI_Number_Test"));

        result.Succeeded.Should().BeTrue();
        result.Value.Should().BeOfType<CalcifiedNumber>().Which.Inner.Should().BeSameAs(mockNumber);
    }

    [Fact]
    public void Parse_Location_InnerValueWrappedInCalcifiedLocation()
    {
        var mockLocation = new MockLocation();
        ParserLookup.AddRuneParser("YI_Location_Test", new MockParser<ILocation>(mockLocation));

        var result = RuneParsingDispatcher.ParseNextRune<ILocation>(new TokenStream("YI YI_Location_Test"));

        result.Succeeded.Should().BeTrue();
        result.Value.Should().BeOfType<CalcifiedLocation>().Which.Inner.Should().BeSameAs(mockLocation);
    }

    [Fact]
    public void Parse_RestoresModeAfterParsing()
    {
        var mockEntitySet = new MockEntitySet();
        ParserLookup.AddRuneParser("YI_ModeRestore_Test", new MockParser<IEntitySet>(mockEntitySet));
        var tokenStream = new TokenStream("YI YI_ModeRestore_Test");

        RuneParsingDispatcher.ParseNextRune<IEntitySet>(tokenStream);

        tokenStream.LivenessMode.Should().Be(LivenessMode.Calcified);
    }

    [Fact]
    public void Parse_ResultIsNotLive()
    {
        var mockEntitySet = new MockEntitySet();
        ParserLookup.AddRuneParser("YI_NotLive_Test", new MockParser<IEntitySet>(mockEntitySet));

        var result = RuneParsingDispatcher.ParseNextRune<IEntitySet>(new TokenStream("YI YI_NotLive_Test"));

        result.IsLive.Should().BeFalse();
    }

    [Fact]
    public void Parse_InsideSAContext_ProducesCalcifiedEntitySet_WithLiveInner()
    {
        // YI SA LA OH: YI re-calcifies around a live SA expression.
        // The outer CalcifiedEntitySet wraps an LA whose inner (OH) was parsed live (not wrapped).
        var (_, result) = SpellParser.Parse("ZU VUN YI SA LA OH HET");

        result.Succeeded.Should().BeTrue();
        var vun = ((RunicMagic.World.Runes.ExecutionRunes.ZU)result.Value)
            .Statement.Should().BeOfType<RunicMagic.World.Runes.EffectRunes.VUN>().Subject;

        var la = vun.ToMove
            .Should().BeOfType<CalcifiedEntitySet>().Which
            .Inner.Should().BeOfType<RunicMagic.World.Execution.EntitySetSelectionCostResolver>().Which
            .Inner.Should().BeOfType<RunicMagic.World.Runes.EntitySetRunes.LA>().Subject;

        // Inner OH was parsed in Live mode (due to SA), so it is not wrapped in CalcifiedEntitySet.
        la.ToGetScopeOf.Should().BeOfType<RunicMagic.World.Runes.EntityReferenceRunes.OH>();
    }

    [Fact]
    public void Parse_WithFailingInner_Fails()
    {
        var result = RuneParsingDispatcher.ParseNextRune<IEntitySet>(new TokenStream("YI"));

        result.Succeeded.Should().BeFalse();
    }
}
