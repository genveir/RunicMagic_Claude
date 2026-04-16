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

    public bool IntersectsWith(Rectangle other)
    {
        var result = X - Width / 2 < other.X + other.Width / 2 &&
            other.X - other.Width / 2 < X + Width / 2 &&
            Y - Height / 2 < other.Y + other.Height / 2 &&
            other.Y - other.Height / 2 < Y + Height / 2;
        return result;
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
