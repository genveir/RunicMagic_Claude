using FluentAssertions;
using RunicMagic.Tests.Builders;
using RunicMagic.World.Runes.EntitySetRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.EntitySetRunes;

public class LATests
{
    [Fact]
    public void Resolve_ReturnsScopeOfInputSet()
    {
        var scopeMember1 = new EntityBuilder().WithLocation(x: 10, y: 0).Build();
        var scopeMember2 = new EntityBuilder().WithLocation(x: 20, y: 0).Build();
        var container = new EntityBuilder().Build();
        container.Scope = () => [scopeMember1, scopeMember2];

        var inputSet = new FixedEntitySet(container);
        var la = new LA(inputSet);
        var context = TestFixtures.MakeContext();

        var result = la.Resolve(context);

        result.Entities.Should().BeEquivalentTo([scopeMember1, scopeMember2]);
    }

    [Fact]
    public void Resolve_DeduplicatesAcrossMultipleInputEntities()
    {
        var shared = new EntityBuilder().Build();
        var unique = new EntityBuilder().Build();
        var entity1 = new EntityBuilder().Build();
        var entity2 = new EntityBuilder().Build();
        entity1.Scope = () => [shared, unique];
        entity2.Scope = () => [shared];

        var inputSet = new FixedEntitySet(entity1, entity2);
        var la = new LA(inputSet);
        var context = TestFixtures.MakeContext();

        var result = la.Resolve(context);

        result.Entities.Should().HaveCount(2);
        result.Entities.Should().Contain(shared);
        result.Entities.Should().Contain(unique);
    }

    [Fact]
    public void Resolve_EntityWithNoScope_ContributesEmptySet()
    {
        var entity = new EntityBuilder().Build();
        var inputSet = new FixedEntitySet(entity);
        var la = new LA(inputSet);
        var context = TestFixtures.MakeContext();

        var result = la.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_WindowOpen_AddsExpansionOutputToResolutionCount()
    {
        var scopeMember = new EntityBuilder().Build();
        var container = new EntityBuilder().Build();
        container.Scope = () => [scopeMember];

        var la = new LA(new FixedEntitySet(container));
        var context = TestFixtures.MakeContext();
        context.OpenResolutionWindow();

        la.Resolve(context);

        context.EntityResolutionCount.Should().Contain(scopeMember.Id);
    }

    [Fact]
    public void Resolve_WindowOpen_BothInputAndExpansionOutputAreTracked()
    {
        var scopeMember = new EntityBuilder().Build();
        var container = new EntityBuilder().Build();
        container.Scope = () => [scopeMember];

        var la = new LA(new FixedEntitySet(container));
        var context = TestFixtures.MakeContext();
        context.OpenResolutionWindow();

        la.Resolve(context);

        // container is added by the inner leaf (FixedEntitySet); scopeMember is added by LA's expansion
        context.EntityResolutionCount.Should().Contain(container.Id);
        context.EntityResolutionCount.Should().Contain(scopeMember.Id);
    }
}
