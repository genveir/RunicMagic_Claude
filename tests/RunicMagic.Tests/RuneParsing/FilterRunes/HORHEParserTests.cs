using FluentAssertions;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.RuneParsing.FilterRunes;
using RunicMagic.Tests.RuneParsing;
using RunicMagic.World.Runes.EntityReferenceRunes;
using RunicMagic.World.Runes.FilterRunes;
using RunicMagic.World.Runes.RuneTypes;
using Xunit;

namespace RunicMagic.Tests.RuneParsing.FilterRunes;

public class HORHEParserTests
{
    [Fact]
    public void ResolvesFromParserLookup()
    {
        var parser = ParserLookup.FindRuneParserByName<IEntitySet>("HORHE");

        parser.Should().BeOfType<HORHEParser>();
    }

    [Fact]
    public void Parse_WithSourceAndExplicitOrigin_ProducesCorrectHORHE()
    {
        var mockSource = new MockEntitySet();
        var mockOrigin = new MockEntitySet();
        ParserLookup.AddRuneParser("HORHE_WithExplicitOrigin_Source_IEntitySet", new MockParser<IEntitySet>(mockSource));
        ParserLookup.AddRuneParser("HORHE_WithExplicitOrigin_Origin_IEntitySet", new MockParser<IEntitySet>(mockOrigin));

        var result = new HORHEParser().Parse(new TokenStream("HORHE_WithExplicitOrigin_Source_IEntitySet HORHE_WithExplicitOrigin_Origin_IEntitySet"));

        result.Succeeded.Should().BeTrue();
        var horhe = result.Value.Should().BeOfType<HORHE>().Subject;
        horhe.Source.Should().BeSameAs(mockSource);
        horhe.Origin.Should().BeSameAs(mockOrigin);
    }

    [Fact]
    public void Parse_WithSourceOnly_UsesOHAsDefaultOrigin()
    {
        var mockSource = new MockEntitySet();
        ParserLookup.AddRuneParser("HORHE_DefaultOrigin_IEntitySet", new MockParser<IEntitySet>(mockSource));

        var result = new HORHEParser().Parse(new TokenStream("HORHE_DefaultOrigin_IEntitySet"));

        result.Succeeded.Should().BeTrue();
        var horhe = result.Value.Should().BeOfType<HORHE>().Subject;
        horhe.Source.Should().BeSameAs(mockSource);
        horhe.Origin.Should().BeOfType<OH>();
    }

    [Fact]
    public void Parse_WithMissingSource_Fails()
    {
        var result = new HORHEParser().Parse(new TokenStream(""));

        result.Succeeded.Should().BeFalse();
    }
}
