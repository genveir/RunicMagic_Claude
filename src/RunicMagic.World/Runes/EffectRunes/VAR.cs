using RunicMagic.World.Execution;
using RunicMagic.World.Motion.Engine;
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

        if (!toMove.Entities.Any())
        {
            return;
        }

        var effect = new LinearEngineMotionEffect(
            context: context,
            toMove: ToMove,
            origin: Origin,
            totalDistanceMm: distance,
            tickCount: Constants.DefaultEffectLength,
            isAway: false,
            effectName: "VAR"
        );
        context.EngineAPI.EngineMotion.AddMotionEffect(effect);
    }

    public override string ToString()
    {
        var result = $"VAR ( {ToMove}, {HowFar}, {Origin} )";
        return result;
    }
}
