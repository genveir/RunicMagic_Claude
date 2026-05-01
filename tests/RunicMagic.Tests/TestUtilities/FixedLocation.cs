using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Tests.TestUtilities;

// Test utility: a pre-baked location holding an already-resolved Location value.
// Use this in tests that need to supply a known location without going through the parser.
// In production code, use CalcifiedLocation instead.
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
