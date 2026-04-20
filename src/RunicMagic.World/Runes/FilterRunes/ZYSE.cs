using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.FilterRunes
{
    public class ZYSE : IEntitySet
    {
        public IEntitySet Source { get; }

        public ZYSE(IEntitySet source)
        {
            Source = source;
        }

        public EntitySet Resolve(SpellContext context)
        {
            var source = Source.Resolve(context);
            if (!source.Entities.Any())
            {
                return new EntitySet([]);
            }
            var maxWeight = source.Entities.Max(e => e.Weight);
            var heaviest = source.Entities.Where(e => e.Weight == maxWeight).ToList();
            var result = new EntitySet(heaviest);
            return result;
        }

        public override string ToString()
        {
            var result = $"ZYSE ( {Source} )";
            return result;
        }
    }
}
