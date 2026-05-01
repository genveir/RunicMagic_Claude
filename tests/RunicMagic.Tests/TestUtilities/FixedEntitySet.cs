using RunicMagic.World;
using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Tests.TestUtilities;

// Test utility: a pre-baked entity set holding an already-resolved EntitySet.
// Use this in tests that need to supply a known set without going through the parser.
// In production code, use CalcifiedEntitySet instead.
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
