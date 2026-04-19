namespace RunicMagic.World.Geometry;

public readonly record struct Location(double X, double Y)
{
    public double GetDistanceTo(Location origin)
    {
        var dx = X - origin.X;
        var dy = Y - origin.Y;
        var distance = Math.Sqrt(dx * dx + dy * dy);
        return distance;
    }
}
