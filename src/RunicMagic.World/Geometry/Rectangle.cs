namespace RunicMagic.World.Geometry;

// X and Y are the center of the rectangle. Width and Height are the full extents.
public readonly record struct Rectangle(Location Location, double Width, double Height)
{
    private readonly double X = Location.X;
    private readonly double Y = Location.Y;

    public bool Contains(Rectangle other)
    {
        var result = X - Width / 2 <= other.X - other.Width / 2 &&
            other.X + other.Width / 2 <= X + Width / 2 &&
            Y - Height / 2 <= other.Y - other.Height / 2 &&
            other.Y + other.Height / 2 <= Y + Height / 2;
        return result;
    }

    public bool Contains(Location location)
    {
        var x = location.X;
        var y = location.Y;

        var result = X - Width / 2 <= x && x <= X + Width / 2 &&
            Y - Height / 2 <= y && y <= Y + Height / 2;
        return result;
    }

    public bool IntersectsWith(Rectangle other)
    {
        var result = X - Width / 2 < other.X + other.Width / 2 &&
            other.X - other.Width / 2 < X + Width / 2 &&
            Y - Height / 2 < other.Y + other.Height / 2 &&
            other.Y - other.Height / 2 < Y + Height / 2;
        return result;
    }

    // Returns the entry t-parameter along the ray (origin + t*direction) where it first hits this
    // rectangle. Returns false if the ray misses, is parallel and outside, or the entry point is
    // behind the origin (t <= 0).
    public bool IntersectsRay(Location origin, Direction direction, out double t)
    {
        t = 0;
        double left = X - Width / 2.0;
        double right = X + Width / 2.0;
        double bottom = Y - Height / 2.0;
        double top = Y + Height / 2.0;

        double tEnter = double.NegativeInfinity;
        double tExit = double.PositiveInfinity;

        var ox = origin.X;
        var oy = origin.Y;
        var dx = direction.X;
        var dy = direction.Y;

        if (dx != 0)
        {
            var tx0 = (left - ox) / dx;
            var tx1 = (right - ox) / dx;
            if (tx0 > tx1) (tx0, tx1) = (tx1, tx0);
            tEnter = Math.Max(tEnter, tx0);
            tExit = Math.Min(tExit, tx1);
        }
        else if (ox < left || ox > right)
        {
            return false;
        }

        if (dy != 0)
        {
            var ty0 = (bottom - oy) / dy;
            var ty1 = (top - oy) / dy;
            if (ty0 > ty1) (ty0, ty1) = (ty1, ty0);
            tEnter = Math.Max(tEnter, ty0);
            tExit = Math.Min(tExit, ty1);
        }
        else if (oy < bottom || oy > top)
        {
            return false;
        }

        if (tEnter > tExit || tEnter <= 0)
            return false;

        t = tEnter;
        return true;
    }

    // True when the nearest point on this rectangle to (px, py) is within maxDistance.
    // Points inside the rectangle have distance 0 and always satisfy the check.
    public bool IsWithinDistanceFromPoint(Location toCheck, double maxDistance)
    {
        var px = toCheck.X;
        var py = toCheck.Y;

        var left = X - Width / 2;
        var right = X + Width / 2;
        var bottom = Y - Height / 2;
        var top = Y + Height / 2;

        var clampedX = Math.Clamp(px, left, right);
        var clampedY = Math.Clamp(py, bottom, top);

        var dx = px - clampedX;
        var dy = py - clampedY;

        var distanceSquared = dx * dx + dy * dy;
        var result = distanceSquared <= maxDistance * maxDistance;
        return result;
    }
}
