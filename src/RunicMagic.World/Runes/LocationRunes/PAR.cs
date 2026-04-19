using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
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

        public Location Evaluate(SpellContext context)
        {
            var entities = EntitySet.Resolve(context).Entities;
            if (entities.Count == 0)
            {
                return new Location(0, 0);
            }
            var result = entities.Select(e => e.Location).Centroid();
            return result;
        }

        public override string ToString()
        {
            var result = $"PAR ( {EntitySet} )";
            return result;
        }
    }
}
