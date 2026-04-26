using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;
using RunicMagic.World.Services;

namespace RunicMagic.World.Runes.EffectRunes;

// FILL
public class TIORJ : IStatement
{
    public IEntitySet From { get; }
    public IEntitySet To { get; }
    public INumber Amount { get; }

    public TIORJ(IEntitySet from, IEntitySet to, INumber amount)
    {
        From = from;
        To = to;
        Amount = amount;
    }

    public void Execute(SpellContext context)
    {
        var fromSet = From.Resolve(context);
        var toSet = To.Resolve(context);
        var amount = Amount.Evaluate(context).Value;

        var drawn = PowerService.DrawPower(fromSet, amount, context.Result);
        PowerService.FillWithOvercharge(toSet, fromSet, drawn, context);
    }

    public override string ToString()
    {
        var result = $"TIORJ ( {From}, {To}, {Amount} )";
        return result;
    }
}
