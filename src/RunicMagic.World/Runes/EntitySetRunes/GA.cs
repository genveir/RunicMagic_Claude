using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.EntitySetRunes;

public class GA : IEntitySet
{
    public EntitySet Resolve(SpellContext context)
    {
        var selection = context.EngineAPI.EntitySetSelectService.GetAll();
        context.EntityResolutionCount?.UnionWith(selection);

        var result = new EntitySet(selection.Entities);
        return result;
    }
}
