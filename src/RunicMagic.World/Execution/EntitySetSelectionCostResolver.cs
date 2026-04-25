using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Execution
{
    public class EntitySetSelectionCostResolver : IEntitySet
    {
        public IEntitySet Inner { get; }

        public EntitySetSelectionCostResolver(IEntitySet inner)
        {
            Inner = inner;
        }

        public EntitySet Resolve(SpellContext context)
        {
            context.OpenResolutionWindow();
            var resolved = Inner.Resolve(context);

            var cost = CalulateCost(context, resolved);

            context.CloseResolutionWindow();

            var drawn = context.DrawPower(cost);
            if (drawn < cost)
            {
                context.Result.Add(new SelectionCostNotMetEvent(Required: cost, Drawn: drawn));
                return new EntitySet([]);
            }

            return resolved;
        }

        private long CalulateCost(SpellContext context, EntitySet resolved)
        {
            var finalSetCost = CalculateFinalSetCost(context, resolved);
            var breadthCost = CalculateBreadthCost(context);

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
                var maxPower = entity.Reservoir?.Max.Invoke() ?? 0L;
                cost += (maxPower + 999) / 1000;
            }

            return cost;
        }

        private long CalculateBreadthCost(SpellContext context)
        {
            var breadthCount = context.EntityResolutionCount?.Count ?? 0;

            return breadthCount;
        }
    }
}
