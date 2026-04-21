using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.EntitySetRunes
{
    public class PA : IEntitySet
    {
        public IEntitySet ToGetScopeOf { get; }

        public PA(IEntitySet toGetScopeOf)
        {
            ToGetScopeOf = toGetScopeOf;
        }

        public EntitySet Resolve(SpellContext context)
        {
            var inputSet = ToGetScopeOf.Resolve(context);
            if (!inputSet.Entities.Any())
            {
                return new EntitySet([]);
            }

            HashSet<EntityId>? intersection = null;
            var entityById = new Dictionary<EntityId, Entity>();

            foreach (var entity in inputSet.Entities)
            {
                var scope = entity.Scope?.Invoke() ?? [];
                foreach (var member in scope)
                {
                    entityById[member.Id] = member;
                }
                var scopeIds = scope.Select(e => e.Id).ToHashSet();
                if (intersection is null)
                {
                    intersection = scopeIds;
                }
                else
                {
                    intersection.IntersectWith(scopeIds);
                }
            }

            var members = (intersection ?? [])
                .Select(id => entityById[id])
                .ToList();
            var result = new EntitySet(members);
            context.EntityResolutionCount?.UnionWith(result.Entities.Select(e => e.Id));
            return result;
        }

        public override string ToString()
        {
            var result = $"PA ( {ToGetScopeOf} )";
            return result;
        }
    }
}
