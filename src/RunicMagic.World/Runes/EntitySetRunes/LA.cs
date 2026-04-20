using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.EntitySetRunes
{
    // SCOPE OF
    public class LA : IEntitySet
    {
        public IEntitySet ToGetScopeOf { get; }

        public LA(IEntitySet toGetScopeOf)
        {
            ToGetScopeOf = toGetScopeOf;
        }

        public EntitySet Resolve(SpellContext context)
        {
            var inputSet = ToGetScopeOf.Resolve(context);
            var seen = new HashSet<EntityId>();
            var union = new List<Entity>();

            foreach (var entity in inputSet.Entities)
            {
                var scope = entity.Scope?.Invoke() ?? [];
                foreach (var member in scope)
                {
                    if (seen.Add(member.Id))
                    {
                        union.Add(member);
                    }
                }
            }

            var result = new EntitySet(union);
            context.EntityResolutionCount?.UnionWith(result.Entities.Select(e => e.Id));
            return result;
        }

        public override string ToString()
        {
            var result = $"LA ( {ToGetScopeOf} )";
            return result;
        }
    }
}
