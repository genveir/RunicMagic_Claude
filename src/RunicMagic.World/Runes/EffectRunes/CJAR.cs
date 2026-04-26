using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
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
        var origin = Origin.Evaluate(context);

        // Y-axis is down, so negative angle is counterclockwise.
        var theta = -(angleDegrees / 2744.0 * 2 * Math.PI);

        var totalCost = 0L;
        foreach (var entity in toRotate.Entities)
        {
            totalCost += RotationCostCalculator.ComputeEntityCost(entity, origin, Math.Abs(theta));
        }

        var drawn = context.DrawPower(totalCost);
        if (drawn < totalCost)
        {
            context.Result.Add(new EffectNotFiredEvent(
                "CJAR",
                $"Insufficient power: needed {totalCost}, drew {drawn}"
            ));
            return;
        }

        foreach (var entity in toRotate.Entities)
        {
            var dx = entity.Location.X - origin.X;
            var dy = entity.Location.Y - origin.Y;
            var cos = Math.Cos(theta);
            var sin = Math.Sin(theta);
            entity.Location = new Location(
                origin.X + dx * cos - dy * sin,
                origin.Y + dx * sin + dy * cos
            );
            entity.Angle += theta;

            if (angleDegrees > 0)
            {
                context.Result.Add(new EntityRotatedEvent(entity, angleDegrees));
            }
        }
    }

    public override string ToString()
    {
        var result = $"CJAR ( {ToRotate}, {HowMuch}, {Origin} )";
        return result;
    }
}
