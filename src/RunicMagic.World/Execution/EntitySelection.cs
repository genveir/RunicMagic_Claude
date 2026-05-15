using RunicMagic.World.Engine;
using RunicMagic.World.Entities;

namespace RunicMagic.World.Execution;

public readonly record struct EntitySelection(HashSet<EntityId> EntityIds, long FictionalResults)
{
    public EntitySelection UnionWith(EntitySelection other)
    {
        var newIds = new HashSet<EntityId>(EntityIds);
        foreach (var id in other.EntityIds)
        {
            newIds.Add(id);
        }
        return new EntitySelection(newIds, Math.Max(FictionalResults, other.FictionalResults));
    }

    public EntitySelection UnionWith(EntitySetSelectionResult selectionResult) =>
        UnionWith(new EntitySelection(selectionResult.Entities.Select(e => e.Id).ToHashSet(), selectionResult.FictionalResults));
}
