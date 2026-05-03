using RunicMagic.World.Entities;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Execution;

public class EntitySetSelectionCostResolver : IEntitySet
{
    public IEntitySet Inner { get; }

    private readonly HashSet<EntityId> _previouslySelectedEntities = new();
    private readonly HashSet<EntityId> _previouslySeenInBreadth = new();

    public EntitySetSelectionCostResolver(IEntitySet inner)
    {
        Inner = inner;
    }

    public EntitySet Resolve(SpellContext context)
    {
        context.OpenResolutionWindow();
        var resolved = Inner.Resolve(context);

        var newBreadthIds = GetNewBreadthIds(context);
        var cost = CalculateCost(context, resolved, newBreadthIds);

        context.CloseResolutionWindow();

        var drawn = context.DrawPower(cost);
        if (drawn < cost)
        {
            context.EventTracker.Add(new SelectionCostNotMetEvent(Required: cost, Drawn: drawn));
            return new EntitySet([]);
        }

        foreach (var id in newBreadthIds)
        {
            _previouslySeenInBreadth.Add(id);
        }

        foreach (var entity in resolved.Entities)
        {
            _previouslySelectedEntities.Add(entity.Id);
        }

        return resolved;
    }

    private HashSet<EntityId> GetNewBreadthIds(SpellContext context)
    {
        var allSeen = context.EntityResolutionCount ?? [];
        var newIds = new HashSet<EntityId>();
        foreach (var id in allSeen)
        {
            if (!_previouslySeenInBreadth.Contains(id))
            {
                newIds.Add(id);
            }
        }
        return newIds;
    }

    private long CalculateCost(SpellContext context, EntitySet resolved, HashSet<EntityId> newBreadthIds)
    {
        var finalSetCost = CalculateFinalSetCost(context, resolved);
        var breadthCost = CalculateBreadthCost(newBreadthIds);

        return finalSetCost + breadthCost;
    }

    private long CalculateFinalSetCost(SpellContext context, EntitySet resolved)
    {
        EntityId[] exemptIds = context.Caster.Entities
            .Concat(context.Executor.Entities)
            .Select(e => e.Id)
            .ToArray();

        var cost = 0L;
        foreach (var entity in resolved.Entities)
        {
            if (exemptIds.Contains(entity.Id))
            {
                continue;
            }
            if (_previouslySelectedEntities.Contains(entity.Id))
            {
                continue;
            }
            var maxPower = entity.Reservoir?.Max.Invoke() ?? 0L;
            cost += (maxPower + 999999999) / 1000000000;
        }

        return cost;
    }

    private static long CalculateBreadthCost(HashSet<EntityId> newBreadthIds)
    {
        return newBreadthIds.Count * 1000000;
    }
}
