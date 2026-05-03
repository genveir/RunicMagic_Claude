using RunicMagic.World.Execution;
using RunicMagic.World.Motion.Engine;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.EffectRunes;

// ROTATE COUNTERCLOCKWISE
public class CJAR : IStatement
{
    public IEntitySet ToRotate { get; }
    public INumber HowMuch { get; }
    public ILocation Origin { get; }

    public CJAR(IEntitySet toRotate, INumber howMuch, ILocation origin)
    {
        ToRotate = toRotate;
        HowMuch = howMuch;
        Origin = origin;
    }

    public void Execute(SpellContext context)
    {
        var toRotate = ToRotate.Resolve(context);
        var angleDegrees = HowMuch.Evaluate(context).Value;

        if (!toRotate.Entities.Any() || angleDegrees <= 0)
        {
            return;
        }

        // Y-axis is down, so negative angle is counterclockwise.
        var totalTheta = -(angleDegrees / 2744.0 * 2 * Math.PI);
        var perTickTheta = totalTheta / 56.0;

        var effect = new RotationEngineMotionEffect(
            context: context,
            toMove: ToRotate,
            origin: Origin,
            perTickTheta: perTickTheta,
            totalRuneDegrees: angleDegrees,
            effectName: "CJAR"
        );
        context.World.AddMotionEffect(effect);
    }

    public override string ToString()
    {
        var result = $"CJAR ( {ToRotate}, {HowMuch}, {Origin} )";
        return result;
    }
}
