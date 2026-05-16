using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.EntitySetRunes;

// NEAR — all entities within howFar mm of any entity in origin set (nearest bounding edge to nearest bounding edge)
public class HORO : IEntitySet
{
    public INumber HowFar { get; }
    public IEntitySet Origin { get; }

    public HORO(INumber howFar, IEntitySet origin)
    {
        HowFar = howFar;
        Origin = origin;
    }

    public EntitySet Resolve(SpellContext context)
    {
        var radius = HowFar.Evaluate(context);
        var originSet = Origin.Resolve(context);
        var originRects = originSet.Entities.Select(e => e.Bounds).ToList();

        var selection = context.EngineAPI.EntitySetSelectService
            .GetAllInRangeFrom(originSet.Entities, radius.Value);

        var result = new EntitySet(selection.Entities);
        context.EntityResolutionCount?.UnionWith(selection);
        return result;
    }

    public override string ToString()
    {
        var result = $"HORO ( {HowFar}, {Origin} )";
        return result;
    }
}
