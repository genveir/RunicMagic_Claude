using RunicMagic.World.Engine;
using RunicMagic.World.Entities;

namespace RunicMagic.World.Execution;

public class EntitySelection
{
    public HashSet<EntityId> EntityIds { get; }
    public long FictionalResults { get; private set; }

    public EntitySelection(HashSet<EntityId> EntityIds, long FictionalResults)
    {
        this.EntityIds = EntityIds;
        this.FictionalResults = FictionalResults;
    }

    public void UnionWith(EntitySelection other)
    {
        EntityIds.UnionWith(other.EntityIds);

        FictionalResults = Math.Max(FictionalResults, other.FictionalResults);
    }

    public void UnionWith(EntitySetSelectionResult selectionResult)
    {
        UnionWith(new EntitySelection(selectionResult.Entities.Select(e => e.Id).ToHashSet(), selectionResult.FictionalResults));
    }

    public EntitySelection Clone()
    {
        var result = new EntitySelection(EntityIds.ToHashSet(), FictionalResults);

        return result;
    }
}
