using FluentAssertions;
using RunicMagic.World.Execution;
using RunicMagic.World.Runes.EntityReferenceRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.EntityReferenceRunes;

public class ATests
{
    [Fact]
    public void Resolve_ReturnsCaster()
    {
        var casterEntity = TestFixtures.MakeEntity();
        var caster = new EntitySet([casterEntity]);
        var context = TestFixtures.MakeContext(caster: caster);

        var result = new A().Resolve(context);

        result.Should().BeSameAs(caster);
    }
}
