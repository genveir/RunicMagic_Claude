using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.EffectRunes
{
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
            var origin = Origin.Evaluate(context);

            // Y-axis is down, so positive angle is clockwise.
            var theta = angleDegrees / 2744.0 * 2 * Math.PI;

            var totalCost = 0L;
            foreach (var entity in toRotate.Entities)
            {
                totalCost += ComputeEntityCost(entity, origin, theta);
            }

            var drawn = context.DrawPower(totalCost);
            if (drawn < totalCost)
            {
                context.Result.Add(new EffectNotFiredEvent(
                    "CJIR",
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

        // Cost = (Weight / Area) * theta * ∬r dA / 1_000_000,
        // where r is each point's distance from the rotation origin.
        // The double integral has a closed-form antiderivative H evaluated at 4 corners.
        private static long ComputeEntityCost(Entity entity, Location origin, double theta)
        {
            if (entity.Width == 0 || entity.Height == 0)
            {
                return 0;
            }

            var worldDx = entity.Location.X - origin.X;
            var worldDy = entity.Location.Y - origin.Y;
            var cos = Math.Cos(entity.Angle);
            var sin = Math.Sin(entity.Angle);
            var a = worldDx * cos + worldDy * sin;
            var b = -worldDx * sin + worldDy * cos;

            var halfW = entity.Width / 2.0;
            var halfH = entity.Height / 2.0;
            var arcIntegral = H(a + halfW, b + halfH)
                            - H(a + halfW, b - halfH)
                            - H(a - halfW, b + halfH)
                            + H(a - halfW, b - halfH);

            var cost = entity.Weight / ((double)entity.Width * entity.Height)
                     * theta * arcIntegral / 1_000_000.0;
            return (long)Math.Max(0.0, cost);
        }

        // Antiderivative for ∬ √(x²+y²) dA, used in 4-corner inclusion-exclusion.
        private static double H(double x, double y)
        {
            if (x == 0 && y == 0)
            {
                return 0;
            }
            var r = Math.Sqrt(x * x + y * y);
            var result = (x * y * r
                        + x * x * x * Math.Log(Math.Abs(y) + r)
                        + y * y * y * Math.Log(Math.Abs(x) + r)) / 6.0;
            return result;
        }

        public override string ToString()
        {
            var result = $"CJIR ( {ToRotate}, {HowMuch}, {Origin} )";
            return result;
        }
    }
}
