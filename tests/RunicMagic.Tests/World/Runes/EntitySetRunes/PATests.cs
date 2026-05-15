using RunicMagic.World.Engine;
using RunicMagic.World.Runes.EntitySetRunes;

namespace RunicMagic.Tests.World.Runes.EntitySetRunes;

public class PATests
{
    [Fact]
    public void Resolve_SingleInputEntity_ReturnsThatEntitysScope()
    {
        var scopeMember1 = new EntityBuilder().Build();
        var scopeMember2 = new EntityBuilder().Build();
        var container = new EntityBuilder().Build();
        container.Scope = () => new EntitySetSelectionResult(Entities: [scopeMember1, scopeMember2], FictionalResults: 0);

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
        entity1.Scope = () => new EntitySetSelectionResult(Entities: [shared, uniqueToFirst], FictionalResults: 0);
        entity2.Scope = () => new EntitySetSelectionResult(Entities: [shared], FictionalResults: 0);

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
        entity1.Scope = () => new EntitySetSelectionResult(Entities: [new EntityBuilder().Build()], FictionalResults: 0);
        entity2.Scope = () => new EntitySetSelectionResult(Entities: [new EntityBuilder().Build()], FictionalResults: 0);

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
        withScope.Scope = () => new EntitySetSelectionResult(Entities: [shared], FictionalResults: 0);

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
        entity1.Scope = () => new EntitySetSelectionResult(Entities: [shared], FictionalResults: 0);
        entity2.Scope = () => new EntitySetSelectionResult(Entities: [shared], FictionalResults: 0);

        var pa = new PA(new FixedEntitySet(entity1, entity2));
        var context = TestFixtures.MakeContext();
        context.OpenResolutionWindow();

        pa.Resolve(context);

        context.EntityResolutionCount!.EntityIds.Should().Contain(shared.Id);
    }
}
