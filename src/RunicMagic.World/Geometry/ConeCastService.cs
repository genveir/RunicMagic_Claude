using RunicMagic.World.Entities;

namespace RunicMagic.World.Geometry;

public class ConeCastService
{
    private readonly WorldModel world;

    public ConeCastService(WorldModel world)
    {
        this.world = world;
    }

    public IReadOnlyList<Entity> Cast(Entity origin, Direction direction, double halfAngle, double range, bool skipTranslucent = true)
    {
        var originBounds = origin.Bounds;
        var originCentre = origin.Location;

        var candidatesWithDistance = world.GetAll()
            .Where(e => e != origin)
            .Where(e => !skipTranslucent || !e.IsTranslucent)
            .Select(e => (Entity: e, Distance: e.Bounds.GetDistanceFromRectangle(originBounds)));

        var candidates = candidatesWithDistance
            .Where(e => e.Distance < range)
            .OrderBy(e => e.Distance)
            .Select(e => e.Entity)
            .ToList();

        // Sorted, merged list of occlusion intervals maintained by InsertOcclusionInterval.
        var occlusionIntervals = new List<(double Min, double Max)>();
        var results = new List<Entity>();

        foreach (var entity in candidates)
        {
            var interval = GetAngularInterval(originCentre, direction, entity.Bounds);
            if (interval is null) continue;

            var (minAngle, maxAngle) = interval.Value;

            if (!IsFullyOccluded(minAngle, maxAngle, occlusionIntervals))
            {
                if (minAngle <= halfAngle && maxAngle >= -halfAngle)
                {
                    results.Add(entity);
                }
            }

            if (!entity.IsTranslucent)
            {
                InsertOcclusionInterval(minAngle, maxAngle, occlusionIntervals);
            }
        }

        return results;
    }

    // Returns the angular interval [min, max] of the entity's bounds relative to the cone direction,
    // or null if the entity centre is behind the origin (negative dot product for all corners).
    private static (double Min, double Max)? GetAngularInterval(Location origin, Direction direction, Rectangle bounds)
    {
        var corners = bounds.GetCorners();
        var minAngle = double.MaxValue;
        var maxAngle = double.MinValue;
        var anyInFront = false;

        foreach (var (X, Y) in corners)
        {
            var dx = X - origin.X;
            var dy = Y - origin.Y;
            var len = Math.Sqrt(dx * dx + dy * dy);
            if (len == 0) continue;

            var dot = dx / len * direction.X + dy / len * direction.Y;
            if (dot > 0) anyInFront = true;

            var angle = Math.Atan2(
                dx * direction.Y - dy * direction.X,
                dx * direction.X + dy * direction.Y);

            if (angle < minAngle) minAngle = angle;
            if (angle > maxAngle) maxAngle = angle;
        }

        if (!anyInFront) return null;

        return (minAngle, maxAngle);
    }

    // Inserts [newMin, newMax] into the sorted, merged interval list, merging any overlapping
    // neighbours so the list stays normalised.
    private static void InsertOcclusionInterval(double newMin, double newMax, List<(double Min, double Max)> intervals)
    {
        // Binary search for the first interval whose Min >= newMin.
        var lo = 0;
        var hi = intervals.Count;
        while (lo < hi)
        {
            var mid = (lo + hi) / 2;
            if (intervals[mid].Min < newMin)
                lo = mid + 1;
            else
                hi = mid;
        }
        var insertAt = lo;

        // Expand newMin/newMax to absorb any overlapping predecessor.
        if (insertAt > 0 && intervals[insertAt - 1].Max >= newMin)
        {
            insertAt--;
            newMin = Math.Min(newMin, intervals[insertAt].Min);
            newMax = Math.Max(newMax, intervals[insertAt].Max);
            intervals.RemoveAt(insertAt);
        }

        // Absorb any overlapping successors.
        while (insertAt < intervals.Count && intervals[insertAt].Min <= newMax)
        {
            newMax = Math.Max(newMax, intervals[insertAt].Max);
            intervals.RemoveAt(insertAt);
        }

        intervals.Insert(insertAt, (newMin, newMax));
    }

    // The interval list is sorted and merged, so a single linear scan suffices.
    private static bool IsFullyOccluded(double minAngle, double maxAngle, List<(double Min, double Max)> intervals)
    {
        var coveredUpTo = minAngle;

        foreach (var (iMin, iMax) in intervals)
        {
            if (iMin > coveredUpTo + double.Epsilon) break;
            if (iMax > coveredUpTo) coveredUpTo = iMax;
        }

        var result = coveredUpTo >= maxAngle;
        return result;
    }
}
