using RunicMagic.World.Geometry;

namespace RunicMagic.Controller.Models
{
    public readonly record struct WorldCoordinate(double X, double Y)
    {
        public Location ToLocation()
        {
            var location = new Location(X, Y);
            return location;
        }

        public static WorldCoordinate FromLocation(Location location)
        {
            var coordinate = new WorldCoordinate(location.X, location.Y);
            return coordinate;
        }
    }
}
