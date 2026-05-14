using RunicMagic.World.Entities;
using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.EntitySetRunes;

// CONE — all entities within range mm inside a cone projected from the caster in its pointing direction
public class CJODAN : IEntitySet
{
    public INumber Range { get; }
    public INumber HalfAngle { get; }

    public CJODAN(INumber range, INumber halfAngle)
    {
        Range = range;
        HalfAngle = halfAngle;
    }

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

        var range = Range.Evaluate(context);
        var halfAngle = HalfAngle.Evaluate(context);
        // TOT (2744) = one full turn = 2π radians
        var halfAngleRadians = halfAngle.Value * (2.0 * Math.PI / 2744.0);

        var coneCast = new ConeCastService(context.World);
        var castResult = coneCast.Cast(caster, caster.PointingDirection.Value, halfAngleRadians, (double)range.Value);

        var result = new EntitySet(castResult.ToList());
        context.EntityResolutionCount?.UnionWith(result.Entities.Select(e => e.Id));
        return result;
    }

    public override string ToString()
    {
        var result = $"CJODAN ( {Range}, {HalfAngle} )";
        return result;
    }
}
