using RunicMagic.World.Geometry;

namespace RunicMagic.Controller.Models
{
    public readonly record struct WorldCoordinate(long X, long Y)
    {
        public Location ToLocation() => new Location(X, Y);

        public static WorldCoordinate FromLocation(Location location) =>
            new WorldCoordinate((long)Math.Round(location.X), (long)Math.Round(location.Y));
    }
}
