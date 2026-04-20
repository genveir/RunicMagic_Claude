using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.FilterRunes
{
    public class HORHE : IEntitySet
    {
        public IEntitySet Source { get; }
        public ILocation Origin { get; }

        public HORHE(IEntitySet source, ILocation origin)
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
            var minDistance = source.Entities.Min(e => e.GetDistance(origin));
            var closest = source.Entities
                .Where(e => e.GetDistance(origin) == minDistance)
                .ToList();
            var result = new EntitySet(closest);
            return result;
        }

        public override string ToString()
        {
            var result = $"HORHE ( {Source}, {Origin} )";
            return result;
        }
    }
}
