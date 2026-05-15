using RunicMagic.World.Entities;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Execution;

public class EntitySetSelectionCostResolver : IEntitySet
{
    public IEntitySet Inner { get; }

    private readonly HashSet<EntityId> previouslySelectedEntities = new();
    private readonly HashSet<EntityId> previouslySeenInBreadth = new();
    private long previouslySeenFictionalResults = 0;

    public EntitySetSelectionCostResolver(IEntitySet inner)
    {
        Inner = inner;
    }

    public EntitySet Resolve(SpellContext context)
    {
        context.OpenResolutionWindow();
        var resolved = Inner.Resolve(context);

        var newBreadthIds = GetNewBreadthIds(context);
        var newFictionalResults = GetNewFictionalResults(context);

        var cost = CalculateCost(context, resolved, newBreadthIds.Count + newFictionalResults);

        context.CloseResolutionWindow();

        var drawn = context.DrawPower(cost);
        if (drawn < cost)
        {
            context.EventTracker.Add(new SelectionCostNotMetEvent(Required: cost, Drawn: drawn));
            return new EntitySet([]);
        }

        foreach (var id in newBreadthIds)
        {
            previouslySeenInBreadth.Add(id);
        }
        previouslySeenFictionalResults += newFictionalResults;

        foreach (var entity in resolved.Entities)
        {
            previouslySelectedEntities.Add(entity.Id);
        }

        return resolved;
    }

    private long GetNewFictionalResults(SpellContext context)
    {
        if (context.EntityResolutionCount == null)
        {
            return 0;
        }

        var highest = Math.Max(context.EntityResolutionCount.Value.FictionalResults, previouslySeenFictionalResults);

        return highest - previouslySeenFictionalResults;
    }

    private HashSet<EntityId> GetNewBreadthIds(SpellContext context)
    {
        var allSeen = context.EntityResolutionCount?.EntityIds ?? [];
        var newIds = new HashSet<EntityId>();
        foreach (var id in allSeen)
        {
            if (!previouslySeenInBreadth.Contains(id))
            {
                newIds.Add(id);
            }
        }
        return newIds;
    }

    private long CalculateCost(SpellContext context, EntitySet resolved, long newBreadthEntityCount)
    {
        var finalSetCost = CalculateFinalSetCost(context, resolved);
        var breadthCost = CalculateBreadthCost(newBreadthEntityCount);

        return finalSetCost + breadthCost;
    }

    private long CalculateFinalSetCost(SpellContext context, EntitySet resolved)
    {
        EntityId[] exemptIds = context.Caster.Entities
            .Union(context.Executor.Entities)
            .Select(e => e.Id)
            .ToArray();

        var cost = 0L;
        foreach (var entity in resolved.Entities)
        {
            if (exemptIds.Contains(entity.Id))
            {
                continue;
            }
            if (previouslySelectedEntities.Contains(entity.Id))
            {
                continue;
            }
            var maxPower = entity.Reservoir?.Max.Invoke() ?? 0L;
            cost += (maxPower + 999999999) / 1000000000;
        }

        return cost;
    }

    private static long CalculateBreadthCost(long newBreadthEntityCount)
    {
        return newBreadthEntityCount * 1000000;
    }
}
