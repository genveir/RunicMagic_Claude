namespace RunicMagic.World.Geometry;

// X and Y are the center of the rectangle. Width and Height are the full extents.
public readonly record struct Rectangle(Location Location, double Width, double Height, double Angle)
{
    // Translates a world-space point into this rectangle's local frame (centred at origin, unrotated).
    private (double X, double Y) ToLocalFrame(double worldX, double worldY)
    {
        var dx = worldX - Location.X;
        var dy = worldY - Location.Y;
        var cos = Math.Cos(Angle);
        var sin = Math.Sin(Angle);
        var localX = dx * cos + dy * sin;
        var localY = -dx * sin + dy * cos;
        return (localX, localY);
    }

    // Returns a new rectangle with its centre rotated around world origin by the given angle
    // and its own orientation incremented by the same amount.
    public Rectangle Rotate(double angle)
    {
        var cos = Math.Cos(angle);
        var sin = Math.Sin(angle);
        var newX = Location.X * cos - Location.Y * sin;
        var newY = Location.X * sin + Location.Y * cos;
        return new Rectangle(new Location(newX, newY), Width, Height, Angle + angle);
    }

    // Returns the 4 corners of this rectangle in world space.
    private (double X, double Y)[] Corners()
    {
        var hw = Width / 2;
        var hh = Height / 2;
        var cos = Math.Cos(Angle);
        var sin = Math.Sin(Angle);
        var cx = Location.X;
        var cy = Location.Y;
        return [
            (cx + hw * cos - hh * sin, cy + hw * sin + hh * cos),
            (cx - hw * cos - hh * sin, cy - hw * sin + hh * cos),
            (cx + hw * cos + hh * sin, cy + hw * sin - hh * cos),
            (cx - hw * cos + hh * sin, cy - hw * sin - hh * cos),
        ];
    }

    public bool Contains(Rectangle other)
    {
        var relative = other with { Location = new Location(other.Location.X - Location.X, other.Location.Y - Location.Y) };
        var local = relative.Rotate(-Angle);
        var halfW = Width / 2;
        var halfH = Height / 2;
        var result = local.Corners().All(c => Math.Abs(c.X) <= halfW && Math.Abs(c.Y) <= halfH);
        return result;
    }

    public bool Contains(Location location)
    {
        var (lx, ly) = ToLocalFrame(location.X, location.Y);
        var result = Math.Abs(lx) <= Width / 2 && Math.Abs(ly) <= Height / 2;
        return result;
    }

    public bool IntersectsWith(Rectangle other)
    {
        // Broad-phase: (Width + Height) / 2 is a conservative circumradius (always >= true circumradius).
        // Squaring both sides avoids any sqrt.
        var cdx = other.Location.X - Location.X;
        var cdy = other.Location.Y - Location.Y;
        var distSq = cdx * cdx + cdy * cdy;
        var sumR = (Width + Height) / 2 + (other.Width + other.Height) / 2;
        if (distSq > sumR * sumR) return false;

        // Narrow-phase: containment check first (handles one rect fully inside the other),
        // then edge-edge intersection.
        var cornersA = Corners();
        var cornersB = other.Corners();

        foreach (var c in cornersB)
            if (Contains(new Location(c.X, c.Y))) return true;
        foreach (var c in cornersA)
            if (other.Contains(new Location(c.X, c.Y))) return true;

        for (var i = 0; i < 4; i++)
        {
            for (var j = 0; j < 4; j++)
            {
                if (SegmentsIntersect(cornersA[i], cornersA[(i + 1) % 4], cornersB[j], cornersB[(j + 1) % 4]))
                    return true;
            }
        }

        return false;
    }

    private static double Cross(
        (double X, double Y) lineStart,
        (double X, double Y) lineEnd,
        (double X, double Y) point)
    {
        var result = (lineEnd.X - lineStart.X) * (point.Y - lineStart.Y)
                   - (lineEnd.Y - lineStart.Y) * (point.X - lineStart.X);
        return result;
    }

    // Returns true only for proper crossings; collinear/touching segments return false.
    private static bool SegmentsIntersect(
        (double X, double Y) a, (double X, double Y) b,
        (double X, double Y) c, (double X, double Y) d)
    {
        var d1 = Cross(c, d, a);
        var d2 = Cross(c, d, b);
        var d3 = Cross(a, b, c);
        var d4 = Cross(a, b, d);
        var result = (d1 > 0 && d2 < 0 || d1 < 0 && d2 > 0) &&
                     (d3 > 0 && d4 < 0 || d3 < 0 && d4 > 0);
        return result;
    }

    // Returns the entry t-parameter along the ray (origin + t*direction) where it first hits this
    // rectangle. Returns false if the ray misses, is parallel and outside, or the entry point is
    // behind the origin (t <= 0).
    public bool IntersectsRay(Location origin, Direction direction, out double t)
    {
        t = 0;
        double left = -Width / 2.0;
        double right = Width / 2.0;
        double bottom = -Height / 2.0;
        double top = Height / 2.0;

        double tEnter = double.NegativeInfinity;
        double tExit = double.PositiveInfinity;

        var (ox, oy) = ToLocalFrame(origin.X, origin.Y);
        var cos = Math.Cos(Angle);
        var sin = Math.Sin(Angle);
        var dx = direction.X * cos + direction.Y * sin;
        var dy = -direction.X * sin + direction.Y * cos;

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

    // Returns the distance from the nearest point on this rectangle to the given location.
    // Points inside the rectangle have distance 0.
    public double GetDistanceFromPoint(Location toCheck)
    {
        var (px, py) = ToLocalFrame(toCheck.X, toCheck.Y);

        var left = -Width / 2;
        var right = Width / 2;
        var bottom = -Height / 2;
        var top = Height / 2;

        var clampedX = Math.Clamp(px, left, right);
        var clampedY = Math.Clamp(py, bottom, top);

        var dx = px - clampedX;
        var dy = py - clampedY;

        var result = Math.Sqrt(dx * dx + dy * dy);
        return result;
    }

    // True when the nearest point on this rectangle to the given location is within maxDistance.
    // Points inside the rectangle have distance 0 and always satisfy the check.
    public bool IsWithinDistanceFromPoint(Location toCheck, double maxDistance)
    {
        var distance = GetDistanceFromPoint(toCheck);
        var result = distance <= maxDistance;
        return result;
    }
}
