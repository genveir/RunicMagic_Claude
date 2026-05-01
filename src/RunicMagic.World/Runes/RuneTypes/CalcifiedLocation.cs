using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;

namespace RunicMagic.World.Runes.RuneTypes;

public class CalcifiedLocation : ILocation
{
    public ILocation Inner { get; }

    private Location? _cached;

    public CalcifiedLocation(ILocation inner)
    {
        Inner = inner;
    }

    public Location Evaluate(SpellContext context)
    {
        if (_cached == null)
        {
            _cached = Inner.Evaluate(context);
        }
        return _cached.Value;
    }
}
