using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;

namespace RunicMagic.World.Runes.RuneTypes;

public class FixedLocation : ILocation
{
    private readonly Location _location;

    public FixedLocation(Location location)
    {
        _location = location;
    }

    public FixedLocation(double x, double y)
    {
        _location = new Location(x, y);
    }

    public Location Evaluate(SpellContext context)
    {
        return _location;
    }
}
