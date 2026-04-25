using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.FilterRunes
{
    public class FUSE : IEntitySet
    {
        public IEntitySet Source { get; }

        public FUSE(IEntitySet source)
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
            var maxPower = source.Entities.Max(e => e.Reservoir?.Current() ?? 0L);
            var mostPowerful = source.Entities
                .Where(e => (e.Reservoir?.Current() ?? 0L) == maxPower)
                .ToList();
            var result = new EntitySet(mostPowerful);
            return result;
        }

        public override string ToString()
        {
            var result = $"FUSE ( {Source} )";
            return result;
        }
    }
}
