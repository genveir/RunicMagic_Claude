using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.FilterRunes
{
    public class HORIL : IEntitySet
    {
        public IEntitySet Source { get; }
        public INumber Lower { get; }
        public INumber Upper { get; }
        public ILocation Origin { get; }

        public HORIL(IEntitySet source, INumber lower, INumber upper, ILocation origin)
        {
            Source = source;
            Lower = lower;
            Upper = upper;
            Origin = origin;
        }

        public EntitySet Resolve(SpellContext context)
        {
            var source = Source.Resolve(context);
            var lower = Lower.Evaluate(context);
            var upper = Upper.Evaluate(context);
            var origin = Origin.Evaluate(context);
            var filtered = source.Entities
                .Where(e =>
                {
                    var distance = e.GetDistance(origin);
                    return distance > (double)lower.Value && distance < (double)upper.Value;
                })
                .ToList();
            var result = new EntitySet(filtered);
            return result;
        }

        public override string ToString()
        {
            var result = $"HORIL ( {Source}, {Lower}, {Upper}, {Origin} )";
            return result;
        }
    }
}
