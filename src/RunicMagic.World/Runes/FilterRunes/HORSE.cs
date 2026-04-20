using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.FilterRunes
{
    public class HORSE : IEntitySet
    {
        public IEntitySet Source { get; }
        public ILocation Origin { get; }

        public HORSE(IEntitySet source, ILocation origin)
        {
            Source = source;
            Origin = origin;
        }

        public EntitySet Resolve(SpellContext context)
        {
            var source = Source.Resolve(context);
            if (!source.Entities.Any())
            {
                return new EntitySet([]);
            }
            var origin = Origin.Evaluate(context);
            var maxDistance = source.Entities.Max(e => e.GetDistance(origin));
            var farthest = source.Entities
                .Where(e => e.GetDistance(origin) == maxDistance)
                .ToList();
            var result = new EntitySet(farthest);
            return result;
        }

        public override string ToString()
        {
            var result = $"HORSE ( {Source}, {Origin} )";
            return result;
        }
    }
}
