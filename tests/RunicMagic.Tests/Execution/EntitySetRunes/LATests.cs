using FluentAssertions;
using RunicMagic.World.Execution;
using RunicMagic.World.Runes.EntitySetRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.EntitySetRunes;

public class LATests
{
    [Fact]
    public void Resolve_ReturnsScopeOfInputSet()
    {
        var scopeMember1 = TestFixtures.MakeEntity(x: 10);
        var scopeMember2 = TestFixtures.MakeEntity(x: 20);
        var container = TestFixtures.MakeEntity();
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
        var shared = TestFixtures.MakeEntity();
        var unique = TestFixtures.MakeEntity();
        var entity1 = TestFixtures.MakeEntity();
        var entity2 = TestFixtures.MakeEntity();
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
        var entity = TestFixtures.MakeEntity();
        var inputSet = new FixedEntitySet(entity);
        var la = new LA(inputSet);
        var context = TestFixtures.MakeContext();

        var result = la.Resolve(context);

        result.Entities.Should().BeEmpty();
    }
}
