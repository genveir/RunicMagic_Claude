using FluentAssertions;
using RunicMagic.World.Runes.EntitySetRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.EntitySetRunes;

public class PATests
{
    [Fact]
    public void Resolve_SingleInputEntity_ReturnsThatEntitysScope()
    {
        var scopeMember1 = TestFixtures.MakeEntity();
        var scopeMember2 = TestFixtures.MakeEntity();
        var container = TestFixtures.MakeEntity();
        container.Scope = () => [scopeMember1, scopeMember2];

        var pa = new PA(new FixedEntitySet(container));
        var context = TestFixtures.MakeContext();

        var result = pa.Resolve(context);

        result.Entities.Should().BeEquivalentTo([scopeMember1, scopeMember2]);
    }

    [Fact]
    public void Resolve_MultipleInputEntities_ReturnsOnlySharedScopeMembers()
    {
        var shared = TestFixtures.MakeEntity();
        var uniqueToFirst = TestFixtures.MakeEntity();
        var entity1 = TestFixtures.MakeEntity();
        var entity2 = TestFixtures.MakeEntity();
        entity1.Scope = () => [shared, uniqueToFirst];
        entity2.Scope = () => [shared];

        var pa = new PA(new FixedEntitySet(entity1, entity2));
        var context = TestFixtures.MakeContext();

        var result = pa.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(shared);
    }

    [Fact]
    public void Resolve_NoSharedScopeMembers_ReturnsEmpty()
    {
        var entity1 = TestFixtures.MakeEntity();
        var entity2 = TestFixtures.MakeEntity();
        entity1.Scope = () => [TestFixtures.MakeEntity()];
        entity2.Scope = () => [TestFixtures.MakeEntity()];

        var pa = new PA(new FixedEntitySet(entity1, entity2));
        var context = TestFixtures.MakeContext();

        var result = pa.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_EntityWithNoScope_MakesIntersectionEmpty()
    {
        var shared = TestFixtures.MakeEntity();
        var withScope = TestFixtures.MakeEntity();
        var withoutScope = TestFixtures.MakeEntity();
        withScope.Scope = () => [shared];

        var pa = new PA(new FixedEntitySet(withScope, withoutScope));
        var context = TestFixtures.MakeContext();

        var result = pa.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_EmptyInputSet_ReturnsEmpty()
    {
        var pa = new PA(new FixedEntitySet());
        var context = TestFixtures.MakeContext();

        var result = pa.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_WindowOpen_AddsIntersectionOutputToResolutionCount()
    {
        var shared = TestFixtures.MakeEntity();
        var entity1 = TestFixtures.MakeEntity();
        var entity2 = TestFixtures.MakeEntity();
        entity1.Scope = () => [shared];
        entity2.Scope = () => [shared];

        var pa = new PA(new FixedEntitySet(entity1, entity2));
        var context = TestFixtures.MakeContext();
        context.OpenResolutionWindow();

        pa.Resolve(context);

        context.EntityResolutionCount.Should().Contain(shared.Id);
    }
}
