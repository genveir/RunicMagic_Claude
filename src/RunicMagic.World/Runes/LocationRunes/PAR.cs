using RunicMagic.World.Execution;
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
            var avgX = (int)Math.Round(entities.Average(e => (double)e.X));
            var avgY = (int)Math.Round(entities.Average(e => (double)e.Y));
            var result = new Location(avgX, avgY);
            return result;
        }

        public override string ToString()
        {
            var result = $"PAR ( {EntitySet} )";
            return result;
        }
    }
}
