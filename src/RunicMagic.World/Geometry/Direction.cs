namespace RunicMagic.World.Geometry;

public readonly record struct Direction(double X, double Y)
{
    public static Direction FromAngle(double angle)
    {
        var direction = new Direction(X: Math.Cos(angle), Y: Math.Sin(angle));
        return direction;
    }

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

    public double Angle => Math.Atan2(Y, X);

    public double AngularDifferenceTo(Direction other)
    {
        var cross = X * other.Y - Y * other.X;
        var dot = X * other.X + Y * other.Y;
        var angle = Math.Atan2(cross, dot);
        return angle;
    }
}
