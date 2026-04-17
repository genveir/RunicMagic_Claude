using RunicMagic.World.Execution;

namespace RunicMagic.World.Runes.RuneTypes
{
    public interface IEntitySet
    {
        EntitySet Resolve(SpellContext context);
    }
}
