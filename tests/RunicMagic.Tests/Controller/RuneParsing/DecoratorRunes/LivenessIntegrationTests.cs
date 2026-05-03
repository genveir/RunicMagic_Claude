using RunicMagic.Controller.RuneParsing;
using RunicMagic.World.Execution;
using RunicMagic.World.Runes.EffectRunes;
using RunicMagic.World.Runes.EntityReferenceRunes;
using RunicMagic.World.Runes.EntitySetRunes;
using RunicMagic.World.Runes.ExecutionRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Tests.Controller.RuneParsing.DecoratorRunes;

// Integration tests for SA(activate) and YI(calcify) decorator runes.
// These use full spell parses to verify the liveness system end-to-end.
public class LivenessIntegrationTests
{
    [Fact]
    public void SA_EntitySet_ResultIsNotWrappedInCalcifiedEntitySet()
    {
        // SA LA OH: the entity set result is live, so no CalcifiedEntitySet wrapping on the taxed slot.
        var (_, result) = SpellParser.Parse("ZU VUN SA LA OH HET");

        result.Succeeded.Should().BeTrue();
        var vun = result.Value.Should().BeOfType<ZU>().Subject
            .Statement.Should().BeOfType<VUN>().Subject;

        var la = vun.ToMove
            .Should().BeOfType<EntitySetSelectionCostResolver>().Which
            .Inner.Should().BeOfType<LA>().Subject;

        // OH was parsed in Live mode so it has no CalcifiedEntitySet wrapper.
        la.ToGetScopeOf.Should().BeOfType<OH>();
    }

    [Fact]
    public void SA_InnerEntitySetParsedLive_NoCalcifiedWrapperOnSubExpressions()
    {
        // A direct entity set: SA A — just A, no CalcifiedEntitySet wrapping.
        var (_, result) = SpellParser.Parse("ZU VUN SA A HET");

        result.Succeeded.Should().BeTrue();
        var vun = result.Value.Should().BeOfType<ZU>().Subject
            .Statement.Should().BeOfType<VUN>().Subject;

        // SA produces a live set: EntitySetSelectionCostResolver wrapping A directly.
        vun.ToMove.Should().BeOfType<EntitySetSelectionCostResolver>()
            .Which.Inner.Should().BeOfType<A>();
    }

    [Fact]
    public void DefaultMode_EntitySet_IsWrappedInCalcifiedEntitySet()
    {
        // Baseline: no SA/YI → entity set gets CalcifiedEntitySet wrapper.
        var (_, result) = SpellParser.Parse("ZU VUN A HET");

        result.Succeeded.Should().BeTrue();
        var vun = result.Value.Should().BeOfType<ZU>().Subject
            .Statement.Should().BeOfType<VUN>().Subject;

        vun.ToMove.Should().BeOfType<CalcifiedEntitySet>()
            .Which.Inner.Should().BeOfType<EntitySetSelectionCostResolver>()
            .Which.Inner.Should().BeOfType<A>();
    }

    [Fact]
    public void SA_LivenessPropagatesToCompoundParent_ParentNotCalcified()
    {
        // AN(SA A, A): the left arg is live via SA. This forces the entire AN expression to be live
        // (isLive propagates upward), so AN itself is not wrapped in CalcifiedEntitySet.
        var (_, result) = SpellParser.Parse("ZU VUN AN SA A A HET");

        result.Succeeded.Should().BeTrue();
        var vun = result.Value.Should().BeOfType<ZU>().Subject
            .Statement.Should().BeOfType<VUN>().Subject;

        // The taxed set: since the AN is live (due to SA inside), no CalcifiedEntitySet outer.
        var an = vun.ToMove
            .Should().BeOfType<EntitySetSelectionCostResolver>().Which
            .Inner.Should().BeOfType<RunicMagic.World.Runes.SetOperationRunes.AN>().Subject;

        // Left (SA A): live, no CalcifiedEntitySet wrapper.
        an.Left.Should().BeOfType<A>();

        // Right (A): Calcified because it has no SA, but isLive on AN was inherited from left.
        // In Live mode the right was also parsed live (SA switched mode for the whole compound).
        // Actually: SA only covers its immediate argument. The right "A" in AN is parsed
        // after SA's scope ends (SA only takes one argument). After SA, mode is restored to
        // Calcified. So AN's right arg "A" is parsed in Calcified mode → CalcifiedEntitySet(A).
        an.Right.Should().BeOfType<CalcifiedEntitySet>()
            .Which.Inner.Should().BeOfType<A>();
    }

    [Fact]
    public void CJIR_DefaultOrigin_IsLivePARNotWrappedInCalcifiedLocation()
    {
        // CJIR's default origin is SA PAR <target tokens>, making the pivot live.
        // After the change from PAR to SA PAR, the origin should be a direct PAR (not CalcifiedLocation).
        var mockEntitySet = new MockEntitySet();
        var mockNumber = new MockNumber();
        ParserLookup.AddRuneParser("Liveness_CJIR_DefaultOrigin_IEntitySet", new MockParser<IEntitySet>(mockEntitySet));
        ParserLookup.AddRuneParser("Liveness_CJIR_DefaultOrigin_INumber", new MockParser<INumber>(mockNumber));

        var result = new RunicMagic.Controller.RuneParsing.EffectRunes.CJIRParser()
            .Parse(new TokenStream("Liveness_CJIR_DefaultOrigin_IEntitySet Liveness_CJIR_DefaultOrigin_INumber"));

        result.Succeeded.Should().BeTrue();
        var cjir = result.Value.Should().BeOfType<CJIR>().Subject;

        // Default origin goes through SA PAR, so origin is PAR (not CalcifiedLocation).
        var par = cjir.Origin.Should().BeOfType<RunicMagic.World.Runes.LocationRunes.PAR>().Subject;

        // PAR was parsed in Live mode, so its entity set has no CalcifiedEntitySet wrapper.
        par.EntitySet.Should().BeOfType<EntitySetSelectionCostResolver>()
            .Which.Inner.Should().BeSameAs(mockEntitySet);
    }
}
