using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.FilterRunes
{
    public class ZYHE : IEntitySet
    {
        public IEntitySet Source { get; }

        public ZYHE(IEntitySet source)
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
            var minWeight = source.Entities.Min(e => e.Weight);
            var lightest = source.Entities.Where(e => e.Weight == minWeight).ToList();
            var result = new EntitySet(lightest);
            return result;
        }

        public override string ToString()
        {
            var result = $"ZYHE ( {Source} )";
            return result;
        }
    }
}
