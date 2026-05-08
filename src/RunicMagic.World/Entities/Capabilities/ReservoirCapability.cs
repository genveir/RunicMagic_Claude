namespace RunicMagic.World.Entities.Capabilities;

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

    public long GetMaxIncludingScope(Entity entity)
    {
        var entitiesInScope = entity.Scope?.Invoke() ?? [];

        return Max() + entitiesInScope.Sum(e => e.Reservoir?.Max.Invoke() ?? 0);
    }

    public long GetCurrentIncludingScope(Entity entity)
    {
        var entitiesInScope = entity.Scope?.Invoke() ?? [];

        return Current() + entitiesInScope.Sum(e => e.Reservoir?.Current.Invoke() ?? 0);
    }

    public Func<long, ReservoirDraw> Draw { get; init; }
    public Func<long, ReservoirFill> Fill { get; init; }

}
