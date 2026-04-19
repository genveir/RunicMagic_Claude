using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;

namespace RunicMagic.World.Runes.RuneTypes
{
    public interface ILocation
    {
        Location Evaluate(SpellContext context);
    }
}
