using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.EffectRunes;
using RunicMagic.World.Execution;
using RunicMagic.World.Runes.EffectRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Tests.Controller.RuneParsing.EffectRunes;

public class TIORJParserTests
{
    [Fact]
    public void ResolvesFromParserLookup()
    {
        var parser = ParserLookup.FindRuneParserByName<IStatement>("TIORJ");

        parser.Should().BeOfType<TIORJParser>();
    }

    [Fact]
    public void Parse_WithAllArgs_ProducesCorrectTIORJ()
    {
        var mockFrom = new MockEntitySet();
        var mockTo = new MockEntitySet();
        var mockAmount = new MockNumber();
        ParserLookup.AddRuneParser("TIORJ_HappyPath_From_IEntitySet", new MockParser<IEntitySet>(mockFrom));
        ParserLookup.AddRuneParser("TIORJ_HappyPath_To_IEntitySet", new MockParser<IEntitySet>(mockTo));
        ParserLookup.AddRuneParser("TIORJ_HappyPath_Amount_INumber", new MockParser<INumber>(mockAmount));

        var result = new TIORJParser().Parse(new TokenStream("TIORJ_HappyPath_From_IEntitySet TIORJ_HappyPath_To_IEntitySet TIORJ_HappyPath_Amount_INumber"));

        result.Succeeded.Should().BeTrue();
        var tiorj = result.Value.Should().BeOfType<TIORJ>().Subject;
        tiorj.From.Should().BeOfType<CalcifiedEntitySet>()
            .Which.Inner.Should().BeOfType<EntitySetSelectionCostResolver>()
            .Which.Inner.Should().BeSameAs(mockFrom);
        tiorj.To.Should().BeOfType<CalcifiedEntitySet>()
            .Which.Inner.Should().BeOfType<EntitySetSelectionCostResolver>()
            .Which.Inner.Should().BeSameAs(mockTo);
        tiorj.Amount.Should().BeOfType<CalcifiedNumber>().Which.Inner.Should().BeSameAs(mockAmount);
    }

    [Fact]
    public void Parse_WithMissingFrom_Fails()
    {
        var result = new TIORJParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public void Parse_WithMissingTo_Fails()
    {
        var mockFrom = new MockEntitySet();
        ParserLookup.AddRuneParser("TIORJ_MissingTo_From_IEntitySet", new MockParser<IEntitySet>(mockFrom));

        var result = new TIORJParser().Parse(new TokenStream("TIORJ_MissingTo_From_IEntitySet"));

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public void Parse_WithMissingAmount_Fails()
    {
        var mockFrom = new MockEntitySet();
        var mockTo = new MockEntitySet();
        ParserLookup.AddRuneParser("TIORJ_MissingAmount_From_IEntitySet", new MockParser<IEntitySet>(mockFrom));
        ParserLookup.AddRuneParser("TIORJ_MissingAmount_To_IEntitySet", new MockParser<IEntitySet>(mockTo));

        var result = new TIORJParser().Parse(new TokenStream("TIORJ_MissingAmount_From_IEntitySet TIORJ_MissingAmount_To_IEntitySet"));

        result.Succeeded.Should().BeFalse();
    }
}
