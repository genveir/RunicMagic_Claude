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

    [Fact]
    public void Resolve_WindowOpen_AddsCasterIdToResolutionCount()
    {
        var casterEntity = TestFixtures.MakeEntity();
        var context = TestFixtures.MakeContext(caster: new EntitySet([casterEntity]));
        context.OpenResolutionWindow();

        new A().Resolve(context);

        context.EntityResolutionCount.Should().Contain(casterEntity.Id);
    }

    [Fact]
    public void Resolve_WindowNull_DoesNotThrow()
    {
        var casterEntity = TestFixtures.MakeEntity();
        var context = TestFixtures.MakeContext(caster: new EntitySet([casterEntity]));

        var act = () => new A().Resolve(context);

        act.Should().NotThrow();
    }
}
