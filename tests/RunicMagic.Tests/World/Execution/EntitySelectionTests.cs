using RunicMagic.World.Engine;
using RunicMagic.World.Entities;
using RunicMagic.World.Execution;

namespace RunicMagic.Tests.World.Execution;

public class EntitySelectionTests
{
    [Fact]
    public void UnionWith_Selection_UnionsIdsAndTakesMaxFictionalResults()
    {
        var idA = EntityId.New();
        var idB = EntityId.New();
        var idC = EntityId.New();
        var target = new EntitySelection([idA, idB], FictionalResults: 3);
        var other = new EntitySelection([idB, idC], FictionalResults: 7);

        target.UnionWith(other);

        target.EntityIds.Should().BeEquivalentTo([idA, idB, idC]);
        target.FictionalResults.Should().Be(7);
    }

    [Fact]
    public void UnionWith_Selection_KeepsHigherFictionalResultsWhenOtherIsLower()
    {
        var target = new EntitySelection([EntityId.New()], FictionalResults: 10);
        var other = new EntitySelection([EntityId.New()], FictionalResults: 4);

        target.UnionWith(other);

        target.FictionalResults.Should().Be(10);
    }

    [Fact]
    public void UnionWith_SelectionResult_AddsEntityIdsAndMergesFictionalResults()
    {
        var existing = EntityId.New();
        var entity = new EntityBuilder().Build();
        var target = new EntitySelection([existing], FictionalResults: 2);
        var selectionResult = new EntitySetSelectionResult(Entities: [entity], FictionalResults: 9);

        target.UnionWith(selectionResult);

        target.EntityIds.Should().BeEquivalentTo([existing, entity.Id]);
        target.FictionalResults.Should().Be(9);
    }

    [Fact]
    public void Clone_ProducesIndependentCopy()
    {
        var idA = EntityId.New();
        var original = new EntitySelection([idA], FictionalResults: 5);

        var clone = original.Clone();
        clone.EntityIds.Add(EntityId.New());

        clone.FictionalResults.Should().Be(5);
        original.EntityIds.Should().ContainSingle().Which.Should().Be(idA);
    }
}
