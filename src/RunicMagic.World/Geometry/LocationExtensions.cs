namespace RunicMagic.World.Geometry;

using RunicMagic.World;

public static class LocationExtensions
{
    public static Location Translate(this Location origin, Direction direction, double distance)
    {
        var x = origin.X + direction.X * distance;
        var y = origin.Y + direction.Y * distance;
        var result = new Location(x, y);
        return result;
    }

    public static Location Centroid(this IEnumerable<Location> locations)
    {
        var list = locations.ToList();
        var avgX = list.Average(l => l.X);
        var avgY = list.Average(l => l.Y);
        var result = new Location(avgX, avgY);
        return result;
    }

    public static double GetDistance(this Entity entity, Location origin)
    {
        var result = new Rectangle(entity.Location, entity.Width, entity.Height, entity.Angle)
            .GetDistanceFromPoint(origin);
        return result;
    }

    public static IEnumerable<Entity> WithinDistance(this IEnumerable<Entity> entities, Location origin, double distance)
    {
        var result = entities.Where(e =>
            new Rectangle(e.Location, e.Width, e.Height, e.Angle)
                .IsWithinDistanceFromPoint(origin, distance));
        return result;
    }

    // Falls back to unweighted centroid when total weight is zero.
    public static Location WeightedCentroid(this IEnumerable<(Location Location, long Weight)> items)
    {
        var list = items.ToList();
        var totalWeight = list.Sum(i => i.Weight);
        if (totalWeight == 0)
        {
            var result = list.Select(i => i.Location).Centroid();
            return result;
        }
        var x = list.Sum(i => i.Location.X * i.Weight) / totalWeight;
        var y = list.Sum(i => i.Location.Y * i.Weight) / totalWeight;
        var weighted = new Location(x, y);
        return weighted;
    }
}
