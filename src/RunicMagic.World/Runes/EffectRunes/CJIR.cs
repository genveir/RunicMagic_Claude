using RunicMagic.World.Execution;
using RunicMagic.World.Motion.Engine;
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

        if (!toRotate.Entities.Any())
        {
            return;
        }

        var effect = new RotationEngineMotionEffect(
            context: context,
            toMove: ToRotate,
            origin: Origin,
            totalRuneDegrees: angleDegrees,
            tickCount: Constants.DefaultEffectLength,
            isClockwise: true,
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
