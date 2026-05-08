using RunicMagic.World.Execution;

namespace RunicMagic.World.Runes.RuneTypes;

public class CalcifiedEntitySet : IEntitySet
{
    public IEntitySet Inner { get; }

    private EntitySet? cached;

    public CalcifiedEntitySet(IEntitySet inner)
    {
        Inner = inner;
    }

    public EntitySet Resolve(SpellContext context)
    {
        if (cached == null)
        {
            cached = Inner.Resolve(context);
        }
        return cached;
    }
}
