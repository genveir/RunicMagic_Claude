using RunicMagic.World.Execution;

namespace RunicMagic.World.Runes.RuneTypes
{
    public interface ILocation
    {
        Location Evaluate(SpellContext context);
    }
}
