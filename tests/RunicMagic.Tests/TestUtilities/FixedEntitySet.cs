using RunicMagic.World.Entities;
using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Tests.TestUtilities;

// Test utility: a pre-baked entity set holding an already-resolved EntitySet.
// Use this in tests that need to supply a known set without going through the parser.
// In production code, use CalcifiedEntitySet instead.
public class FixedEntitySet : IEntitySet
{
    private readonly EntitySet resolved;

    public FixedEntitySet(EntitySet resolved)
    {
        this.resolved = resolved;
    }

    public FixedEntitySet(params Entity[] entities)
    {
        resolved = new EntitySet(entities);
    }

    public EntitySet Resolve(SpellContext context)
    {
        context.EntityResolutionCount?.UnionWith(resolved.Entities.Select(e => e.Id));
        return resolved;
    }
}
