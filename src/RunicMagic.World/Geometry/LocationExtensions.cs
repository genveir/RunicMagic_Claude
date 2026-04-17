using RunicMagic.World.Execution;

namespace RunicMagic.World.Geometry;

public static class LocationExtensions
{
    public static Location Translate(this Location origin, Direction direction, int distance)
    {
        var x = (int)Math.Round(origin.X + direction.X * distance);
        var y = (int)Math.Round(origin.Y + direction.Y * distance);
        var result = new Location(x, y);
        return result;
    }

    public static Location Centroid(this IEnumerable<Location> locations)
    {
        var list = locations.ToList();
        var avgX = (int)Math.Round(list.Average(l => (double)l.X));
        var avgY = (int)Math.Round(list.Average(l => (double)l.Y));
        var result = new Location(avgX, avgY);
        return result;
    }
}
