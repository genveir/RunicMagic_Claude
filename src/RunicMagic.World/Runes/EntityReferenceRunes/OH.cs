using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.EntityReferenceRunes;

// THIS (executor)
public class OH : IEntitySet
{
    public OH() { }

    public EntitySet Resolve(SpellContext context)
    {
        var result = context.Executor;
        context.EntityResolutionCount?.UnionWith(new EntitySelection(result.Entities.Select(e => e.Id).ToHashSet(), 0));
        return result;
    }

    public override string ToString()
    {
        return "OH";
    }
}
