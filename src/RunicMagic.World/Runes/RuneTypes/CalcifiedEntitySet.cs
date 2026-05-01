using RunicMagic.World.Execution;

namespace RunicMagic.World.Runes.RuneTypes;

public class CalcifiedEntitySet : IEntitySet
{
    public IEntitySet Inner { get; }

    private EntitySet? _cached;

    public CalcifiedEntitySet(IEntitySet inner)
    {
        Inner = inner;
    }

    public EntitySet Resolve(SpellContext context)
    {
        if (_cached == null)
        {
            _cached = Inner.Resolve(context);
        }
        return _cached;
    }
}
