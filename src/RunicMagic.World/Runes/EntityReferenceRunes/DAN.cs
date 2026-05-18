using RunicMagic.World.Engine;
using RunicMagic.World.Entities;
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
            .GetAllInRay(
                from: caster.Location,
                direction: caster.PointingDirection.Value,
                range: 50_000);

        if (rayResult.Entities.Count == 0)
        {
            return new EntitySet([]);
        }

        Entity? hitEntity = null;
        List<Entity> resolvedEntities = [];

        for (int n = 0; n < rayResult.Entities.Count; n++)
        {
            resolvedEntities.Add(rayResult.Entities[n]);

            if (rayResult.Entities[n] == caster)
                continue;

            if (rayResult.Entities[n].IsTranslucent)
                continue;

            hitEntity = rayResult.Entities[n];
        }

        if (hitEntity == null)
        {
            return new EntitySet([]);
        }

        var danResult = new EntitySetSelectionResult(resolvedEntities, 0);

        var result = new EntitySet([hitEntity]);
        context.EntityResolutionCount?.UnionWith(danResult);
        return result;
    }

    public override string ToString()
    {
        return "DAN";
    }
}
