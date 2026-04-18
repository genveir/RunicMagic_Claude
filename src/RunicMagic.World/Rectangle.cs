namespace RunicMagic.World;

// X and Y are the center of the rectangle. Width and Height are the full extents.
public readonly record struct Rectangle(int X, int Y, int Width, int Height)
{
    public bool Contains(Rectangle other)
    {
        var result = X - Width / 2 <= other.X - other.Width / 2 &&
            other.X + other.Width / 2 <= X + Width / 2 &&
            Y - Height / 2 <= other.Y - other.Height / 2 &&
            other.Y + other.Height / 2 <= Y + Height / 2;
        return result;
    }

    public bool Contains(int x, int y)
    {
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
    public bool IntersectsRay(double ox, double oy, double dx, double dy, out double t)
    {
        t = 0;
        double left   = X - Width  / 2.0;
        double right  = X + Width  / 2.0;
        double bottom = Y - Height / 2.0;
        double top    = Y + Height / 2.0;

        double tEnter = double.NegativeInfinity;
        double tExit  = double.PositiveInfinity;

        if (dx != 0)
        {
            var tx0 = (left  - ox) / dx;
            var tx1 = (right - ox) / dx;
            if (tx0 > tx1) (tx0, tx1) = (tx1, tx0);
            tEnter = Math.Max(tEnter, tx0);
            tExit  = Math.Min(tExit,  tx1);
        }
        else if (ox < left || ox > right)
        {
            return false;
        }

        if (dy != 0)
        {
            var ty0 = (bottom - oy) / dy;
            var ty1 = (top    - oy) / dy;
            if (ty0 > ty1) (ty0, ty1) = (ty1, ty0);
            tEnter = Math.Max(tEnter, ty0);
            tExit  = Math.Min(tExit,  ty1);
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

    // Overlap or shared edge — used for the "touch" spatial relationship.
    public bool Touches(Rectangle other)
    {
        var result = X - Width / 2 <= other.X + other.Width / 2 &&
            other.X - other.Width / 2 <= X + Width / 2 &&
            Y - Height / 2 <= other.Y + other.Height / 2 &&
            other.Y - other.Height / 2 <= Y + Height / 2;
        return result;
    }
}
