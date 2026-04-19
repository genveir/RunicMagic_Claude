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
            var resolved = Inner.Resolve(context);

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
                var maxPower = entity.MaxReservoir?.Invoke() ?? 0L;
                cost += (maxPower + 999) / 1000;
            }

            var drawn = context.DrawPower(cost);
            if (drawn < cost)
            {
                context.Result.Add(new SelectionCostNotMetEvent(Required: cost, Drawn: drawn));
                return new EntitySet([]);
            }

            return resolved;
        }
    }
}
