using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.FilterRunes
{
    public class ZYIL : IEntitySet
    {
        public IEntitySet Source { get; }
        public INumber Lower { get; }
        public INumber Upper { get; }

        public ZYIL(IEntitySet source, INumber lower, INumber upper)
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
                .Where(e => e.Weight > lower.Value && e.Weight < upper.Value)
                .ToList();
            var result = new EntitySet(filtered);
            return result;
        }

        public override string ToString()
        {
            var result = $"ZYIL ( {Source}, {Lower}, {Upper} )";
            return result;
        }
    }
}
