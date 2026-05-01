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

        if (!toMove.Entities.Any() || distance <= 0)
        {
            return;
        }

        double perTickDistance = distance / 56.0;

        var effect = new LinearMotionEffect(
            context: context,
            toMove: ToMove,
            origin: Origin,
            perTickDistance: perTickDistance,
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
