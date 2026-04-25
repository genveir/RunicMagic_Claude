using FluentAssertions;
using RunicMagic.Tests.Builders;
using RunicMagic.World.Runes.EntitySetRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.EntitySetRunes;

public class PATests
{
    [Fact]
    public void Resolve_SingleInputEntity_ReturnsThatEntitysScope()
    {
        var scopeMember1 = new EntityBuilder().Build();
        var scopeMember2 = new EntityBuilder().Build();
        var container = new EntityBuilder().Build();
        container.Scope = () => [scopeMember1, scopeMember2];

        var pa = new PA(new FixedEntitySet(container));
        var context = TestFixtures.MakeContext();

        var result = pa.Resolve(context);

        result.Entities.Should().BeEquivalentTo([scopeMember1, scopeMember2]);
    }

    [Fact]
    public void Resolve_MultipleInputEntities_ReturnsOnlySharedScopeMembers()
    {
        var shared = new EntityBuilder().Build();
        var uniqueToFirst = new EntityBuilder().Build();
        var entity1 = new EntityBuilder().Build();
        var entity2 = new EntityBuilder().Build();
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
        var entity1 = new EntityBuilder().Build();
        var entity2 = new EntityBuilder().Build();
        entity1.Scope = () => [new EntityBuilder().Build()];
        entity2.Scope = () => [new EntityBuilder().Build()];

        var pa = new PA(new FixedEntitySet(entity1, entity2));
        var context = TestFixtures.MakeContext();

        var result = pa.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_EntityWithNoScope_MakesIntersectionEmpty()
    {
        var shared = new EntityBuilder().Build();
        var withScope = new EntityBuilder().Build();
        var withoutScope = new EntityBuilder().Build();
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
        var shared = new EntityBuilder().Build();
        var entity1 = new EntityBuilder().Build();
        var entity2 = new EntityBuilder().Build();
        entity1.Scope = () => [shared];
        entity2.Scope = () => [shared];

        var pa = new PA(new FixedEntitySet(entity1, entity2));
        var context = TestFixtures.MakeContext();
        context.OpenResolutionWindow();

        pa.Resolve(context);

        context.EntityResolutionCount.Should().Contain(shared.Id);
    }
}
