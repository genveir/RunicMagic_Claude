using RunicMagic.World.Execution;

namespace RunicMagic.World.Runes.RuneTypes;

public class CalcifiedNumber : INumber
{
    public INumber Inner { get; }

    private Number? _cached;

    public CalcifiedNumber(INumber inner)
    {
        Inner = inner;
    }

    public Number Evaluate(SpellContext context)
    {
        if (_cached == null)
        {
            _cached = Inner.Evaluate(context);
        }
        return _cached.Value;
    }
}
