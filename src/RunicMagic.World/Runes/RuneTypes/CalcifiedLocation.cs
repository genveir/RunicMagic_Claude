using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;

namespace RunicMagic.World.Runes.RuneTypes;

public class CalcifiedLocation : ILocation
{
    public ILocation Inner { get; }

    private Location? cached;

    public CalcifiedLocation(ILocation inner)
    {
        Inner = inner;
    }

    public Location Evaluate(SpellContext context)
    {
        if (cached == null)
        {
            cached = Inner.Evaluate(context);
        }
        return cached.Value;
    }
}
