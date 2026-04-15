namespace RunicMagic.World;

public readonly record struct Rectangle(int X, int Y, int Width, int Height)
{
    public int Right => X + Width;
    public int Bottom => Y + Height;

    public bool Contains(Rectangle other) =>
        other.X >= X && other.Y >= Y &&
        other.Right <= Right && other.Bottom <= Bottom;

    public bool IntersectsWith(Rectangle other) =>
        X < other.Right && other.X < Right &&
        Y < other.Bottom && other.Y < Bottom;

    // Overlap or shared edge — used for the "touch" spatial relationship.
    public bool Touches(Rectangle other) =>
        X <= other.Right && other.X <= Right &&
        Y <= other.Bottom && other.Y <= Bottom;
}
