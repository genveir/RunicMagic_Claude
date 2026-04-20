using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.FilterRunes
{
    public class FUHE : IEntitySet
    {
        public IEntitySet Source { get; }

        public FUHE(IEntitySet source)
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
            var minPower = source.Entities.Min(e => e.CurrentReservoir?.Invoke() ?? 0L);
            var leastPowerful = source.Entities
                .Where(e => (e.CurrentReservoir?.Invoke() ?? 0L) == minPower)
                .ToList();
            var result = new EntitySet(leastPowerful);
            return result;
        }

        public override string ToString()
        {
            var result = $"FUHE ( {Source} )";
            return result;
        }
    }
}
