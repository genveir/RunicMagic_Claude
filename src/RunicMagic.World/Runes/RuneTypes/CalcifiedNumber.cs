using RunicMagic.World.Execution;

namespace RunicMagic.World.Runes.RuneTypes;

public class CalcifiedNumber : INumber
{
    public INumber Inner { get; }

    private Number? cached;

    public CalcifiedNumber(INumber inner)
    {
        Inner = inner;
    }

    public Number Evaluate(SpellContext context)
    {
        if (cached == null)
        {
            cached = Inner.Evaluate(context);
        }
        return cached.Value;
    }
}
