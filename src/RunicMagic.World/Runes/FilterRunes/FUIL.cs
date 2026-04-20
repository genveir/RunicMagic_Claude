using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.FilterRunes
{
    public class FUIL : IEntitySet
    {
        public IEntitySet Source { get; }
        public INumber Lower { get; }
        public INumber Upper { get; }

        public FUIL(IEntitySet source, INumber lower, INumber upper)
        {
            Source = source;
            Lower = lower;
            Upper = upper;
        }

        public EntitySet Resolve(SpellContext context)
        {
            var source = Source.Resolve(context);
            var lower = Lower.Evaluate(context);
            var upper = Upper.Evaluate(context);
            var filtered = source.Entities
                .Where(e =>
                {
                    var current = e.CurrentReservoir?.Invoke() ?? 0L;
                    return current > lower.Value && current < upper.Value;
                })
                .ToList();
            var result = new EntitySet(filtered);
            return result;
        }

        public override string ToString()
        {
            var result = $"FUIL ( {Source}, {Lower}, {Upper} )";
            return result;
        }
    }
}
