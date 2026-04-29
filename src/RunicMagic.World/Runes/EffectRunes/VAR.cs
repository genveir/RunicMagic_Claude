using RunicMagic.World.Execution;
using RunicMagic.World.Motion;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.EffectRunes;

// PULL
public class VAR : IStatement
{
    public IEntitySet ToMove { get; }
    public INumber HowFar { get; }
    public ILocation Origin { get; }

    public VAR(IEntitySet toMove, INumber howFar, ILocation origin)
    {
        ToMove = toMove;
        HowFar = howFar;
        Origin = origin;
    }

    public void Execute(SpellContext context)
    {
        var toMove = ToMove.Resolve(context);
        long distance = HowFar.Evaluate(context).Value;
        var origin = Origin.Evaluate(context);

        if (!toMove.Entities.Any() || distance <= 0)
        {
            return;
        }

        long totalWeight = toMove.Entities.Sum(e => e.Weight);
        long totalCost = distance * totalWeight / 1_000_000;
        long perTickCost = totalCost / 56;
        double perTickDistance = distance / 56.0;

        var fixedEntities = new FixedEntitySet(toMove);
        var fixedOrigin = new FixedLocation(origin);

        var effect = new LinearMotionEffect(
            context: context,
            entities: fixedEntities,
            origin: fixedOrigin,
            perTickDistance: perTickDistance,
            perTickCost: perTickCost,
            totalDistanceMm: distance,
            isAway: false,
            effectName: "VAR"
        );
        context.World.AddMotionEffect(effect);
    }

    public override string ToString()
    {
        var result = $"VAR ( {ToMove}, {HowFar}, {Origin} )";
        return result;
    }
}
