namespace RunicMagic.World.Capabilities;

public readonly record struct ReservoirDraw(long Amount, bool IsDrained);

public readonly record struct ReservoirFill(long Amount, bool IsFull);

public class ReservoirCapability
{
    public ReservoirCapability(
        Func<long> max,
        Func<long> current,
        Func<long, ReservoirDraw> draw,
        Func<long, ReservoirFill> fill)
    {
        Max = max;
        Current = current;
        Draw = draw;
        Fill = fill;
    }


    public Func<long> Max { get; init; }
    public Func<long> Current { get; init; }

    public Func<long, ReservoirDraw> Draw { get; init; }
    public Func<long, ReservoirFill> Fill { get; init; }

}
