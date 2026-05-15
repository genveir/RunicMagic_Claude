using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.EntityReferenceRunes;

// POINTING AT (aimed entity via ray cast)
public class DAN : IEntitySet
{
    public DAN() { }

    public EntitySet Resolve(SpellContext context)
    {
        var casterEntities = context.Caster.Entities;
        if (casterEntities.Count == 0)
        {
            return new EntitySet([]);
        }

        var caster = casterEntities[0];
        if (!caster.PointingDirection.HasValue)
        {
            return new EntitySet([]);
        }

        var rayResult = context.EngineAPI.EntitySetSelectService
            .GetFirstInRay(
                filter: [caster],
                from: caster.Location,
                direction: caster.PointingDirection.Value,
                range: 50_000,
                filterTranslucent: true);

        if (rayResult.Entities.Count == 0)
        {
            return new EntitySet([]);
        }

        var result = new EntitySet([rayResult.Entities.Single()]);
        context.EntityResolutionCount?.UnionWith(rayResult);
        return result;
    }

    public override string ToString()
    {
        return "DAN";
    }
}
