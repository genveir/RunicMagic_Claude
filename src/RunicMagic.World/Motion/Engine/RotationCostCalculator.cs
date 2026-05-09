using RunicMagic.World.Entities;
using RunicMagic.World.Geometry;

namespace RunicMagic.World.Motion.Engine;

public static class RotationCostCalculator
{
    // Cost = (Weight / Area) * theta * ∬r dA,
    // where r is each point's distance from the rotation origin.
    // The double integral has a closed-form antiderivative H evaluated at 4 corners.
    public static long ComputeEntityCost(Entity entity, Location origin, double theta)
    {
        if (entity.Width == 0 || entity.Height == 0)
        {
            return 0;
        }

        var worldDx = entity.Location.X - origin.X;
        var worldDy = entity.Location.Y - origin.Y;
        var cos = Math.Cos(entity.FacingAngle);
        var sin = Math.Sin(entity.FacingAngle);
        var a = worldDx * cos + worldDy * sin;
        var b = -worldDx * sin + worldDy * cos;

        var halfW = entity.Width / 2.0;
        var halfH = entity.Height / 2.0;
        var arcIntegral = H(a + halfW, b + halfH)
                        - H(a + halfW, b - halfH)
                        - H(a - halfW, b + halfH)
                        + H(a - halfW, b - halfH);

        var cost = entity.Weight / ((double)entity.Width * entity.Height)
                 * theta * arcIntegral;
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
}
