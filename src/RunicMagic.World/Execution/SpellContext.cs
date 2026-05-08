using RunicMagic.World.Entities;

namespace RunicMagic.World.Execution;

public class SpellContext
{
    private readonly Stack<EntitySet> sourceStack = new();
    private readonly Stack<HashSet<EntityId>> resolutionStack = new();

    public EntitySet Caster { get; }
    public EntitySet Executor { get; }
    public WorldModel World { get; }
    public IWorldEventTracker EventTracker { get; internal set; }

    public SpellContext(EntitySet caster, EntitySet executor, WorldModel world, IWorldEventTracker eventTracker)
    {
        Caster = caster;
        Executor = executor;
        World = world;
        EventTracker = eventTracker;
    }

    public void PushPowerSource(EntitySet source)
    {
        sourceStack.Push(source);
    }

    public void PopPowerSource()
    {
        sourceStack.Pop();
    }

    public HashSet<EntityId>? EntityResolutionCount =>
        resolutionStack.TryPeek(out var top) ? top : null;

    public void OpenResolutionWindow()
    {
        resolutionStack.Push(new HashSet<EntityId>());
    }

    public void CloseResolutionWindow()
    {
        resolutionStack.Pop();
    }

    public SpellContext ForkWithNewExecutor(EntitySet newExecutor)
    {
        var forked = new SpellContext(Caster, newExecutor, World, EventTracker);
        foreach (var source in sourceStack.Reverse())
        {
            forked.sourceStack.Push(source);
        }
        foreach (var set in resolutionStack.Reverse())
        {
            forked.resolutionStack.Push(new HashSet<EntityId>(set));
        }
        return forked;
    }

    public long DrawPower(long amount)
    {
        var remaining = amount;
        var sources = sourceStack.Concat([Executor.GetScope(), Executor, Caster.GetScope(), Caster]);
        foreach (var source in sources)
        {
            if (remaining == 0)
            {
                break;
            }
            remaining -= PowerService.DrawPower(source, remaining, EventTracker);
        }
        return amount - remaining;
    }
}
