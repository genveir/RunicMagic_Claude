using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.LocationRunes
{
    // CENTERING
    public class GER : ILocation
    {
        public IEntitySet EntitySet { get; }

        public GER(IEntitySet entitySet)
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
            var result = entities
                .Select(e => (e.Location, e.Weight))
                .WeightedCentroid();
            return result;
        }

        public override string ToString()
        {
            var result = $"GER ( {EntitySet} )";
            return result;
        }
    }
}
