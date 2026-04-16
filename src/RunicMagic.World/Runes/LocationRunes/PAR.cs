using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.LocationRunes
{
    // LOCATION OF
    public class PAR : ILocation
    {
        public IEntitySet EntitySet { get; }

        public PAR(IEntitySet entitySet)
        {
            EntitySet = entitySet;
        }
    }
}
