using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.EntityReferenceRunes;

// INDICATING (consciously indicated entity)
public class KAL : IEntitySet
{
    public KAL() { }

    public EntitySet Resolve(SpellContext context)
    {
        var casterEntities = context.Caster.Entities;
        if (casterEntities.Count == 0)
        {
            return new EntitySet([]);
        }

        var caster = casterEntities[0];
        if (caster.IndicateTarget == null)
        {
            return new EntitySet([]);
        }

        // Self-indicate: no raycast needed
        if (caster.IndicateTarget.EntityId == caster.Id)
        {
            var selfResult = new EntitySet([caster]);
            context.EntityResolutionCount?.UnionWith(new EntitySelection([caster.Id], 0));
            return selfResult;
        }

        if (caster.IndicateTarget.Direction == null)
        {
            return new EntitySet([]);
        }

        var rayResult = context.EngineAPI.EntitySetSelectService
            .GetFirstInRay(
                filter: [caster],
                from: caster.Location,
                direction: caster.IndicateTarget.Direction.Value,
                range: 1000,
                filterTranslucent: false);

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
        return "KAL";
    }
}
