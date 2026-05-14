using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.EntitySetRunes;

public class GA : IEntitySet
{
    public EntitySet Resolve(SpellContext context)
    {
        var result = new EntitySet(context.World.GetAll());
        context.EntityResolutionCount?.UnionWith(result.Entities.Select(e => e.Id));
        return result;
    }
}
