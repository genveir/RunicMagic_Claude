using RunicMagic.World.Execution;

namespace RunicMagic.World.Runes.RuneTypes;

public class FixedEntitySet : IEntitySet
{
    private readonly EntitySet _resolved;

    public FixedEntitySet(EntitySet resolved)
    {
        _resolved = resolved;
    }

    public FixedEntitySet(params Entity[] entities)
    {
        _resolved = new EntitySet(entities);
    }

    public EntitySet Resolve(SpellContext context)
    {
        context.EntityResolutionCount?.UnionWith(_resolved.Entities.Select(e => e.Id));
        return _resolved;
    }
}
