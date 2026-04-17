using RunicMagic.World.Execution;

namespace RunicMagic.World.Geometry;

public readonly record struct Direction(double X, double Y)
{
    // Returns a random unit direction when from and to are the same point.
    public static Direction FromPoints(Location from, Location to)
    {
        var dx = (double)(to.X - from.X);
        var dy = (double)(to.Y - from.Y);
        var length = Math.Sqrt(dx * dx + dy * dy);
        if (length == 0.0)
        {
            var angle = Random.Shared.NextDouble() * 2 * Math.PI;
            var random = new Direction(Math.Cos(angle), Math.Sin(angle));
            return random;
        }
        var direction = new Direction(dx / length, dy / length);
        return direction;
    }
}
