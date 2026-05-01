using RunicMagic.World.Execution;
using RunicMagic.World.Motion;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.EffectRunes;

// ROTATE CLOCKWISE
public class CJIR : IStatement
{
    public IEntitySet ToRotate { get; }
    public INumber HowMuch { get; }
    public ILocation Origin { get; }

    public CJIR(IEntitySet toRotate, INumber howMuch, ILocation origin)
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

        // Y-axis is down, so positive angle is clockwise.
        var totalTheta = angleDegrees / 2744.0 * 2 * Math.PI;
        var perTickTheta = totalTheta / 56.0;

        var effect = new RotationMotionEffect(
            context: context,
            entities: ToRotate,
            origin: Origin,
            perTickTheta: perTickTheta,
            totalRuneDegrees: angleDegrees,
            effectName: "CJIR"
        );
        context.World.AddMotionEffect(effect);
    }

    public override string ToString()
    {
        var result = $"CJIR ( {ToRotate}, {HowMuch}, {Origin} )";
        return result;
    }
}
