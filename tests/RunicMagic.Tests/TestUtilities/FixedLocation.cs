using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Tests.TestUtilities;

// Test utility: a pre-baked location holding an already-resolved Location value.
// Use this in tests that need to supply a known location without going through the parser.
// In production code, use CalcifiedLocation instead.
public class FixedLocation : ILocation
{
    private readonly Location location;

    public FixedLocation(Location location)
    {
        this.location = location;
    }

    public FixedLocation(double x, double y)
    {
        location = new Location(x, y);
    }

    public Location Evaluate(SpellContext context)
    {
        return location;
    }
}
