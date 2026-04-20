using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.FilterRunes
{
    public class DRYAL : IEntitySet
    {
        public IEntitySet Source { get; }

        public DRYAL(IEntitySet source)
        {
            Source = source;
        }

        public EntitySet Resolve(SpellContext context)
        {
            var source = Source.Resolve(context);
            var alive = source.Entities
                .Where(e => e.Life != null && e.Life.CurrentHitPoints > 0)
                .ToList();
            var result = new EntitySet(alive);
            return result;
        }

        public override string ToString()
        {
            var result = $"DRYAL ( {Source} )";
            return result;
        }
    }
}
