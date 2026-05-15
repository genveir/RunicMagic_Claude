using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

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

        var drawn = context.EngineAPI.PowerService.DrawPower(
            entitySet: fromSet,
            amount: amount,
            eventTracker: context.EventTracker);

        context.EngineAPI.PowerService.FillWithOvercharge(
            toFill: toSet,
            returnSource: fromSet,
            caster: context.Caster,
            executor: context.Executor,
            amount: drawn, eventTracker:
            context.EventTracker);
    }

    public override string ToString()
    {
        var result = $"TIORJ ( {From}, {To}, {Amount} )";
        return result;
    }
}
