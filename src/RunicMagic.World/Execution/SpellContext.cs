namespace RunicMagic.World.Execution;

public class SpellContext
{
    private readonly Stack<EntitySet> _sourceStack = new();

    public SpellContext(EntitySet caster, EntitySet executor, WorldModel world, SpellResult result)
    {
        Caster = caster;
        Executor = executor;
        World = world;
        Result = result;
    }

    public EntitySet Caster { get; }
    public EntitySet Executor { get; }
    public WorldModel World { get; }
    public SpellResult Result { get; }

    public void PushPowerSource(EntitySet source)
    {
        _sourceStack.Push(source);
    }

    public void PopPowerSource()
    {
        _sourceStack.Pop();
    }

    public SpellContext ForkWithNewExecutor(EntitySet newExecutor)
    {
        var forked = new SpellContext(Caster, newExecutor, World, Result);
        foreach (var source in _sourceStack.Reverse())
        {
            forked._sourceStack.Push(source);
        }
        return forked;
    }

    public long DrawPower(long amount)
    {
        var remaining = amount;
        var sources = _sourceStack.Concat([Executor.GetScope(), Executor, Caster.GetScope(), Caster]);
        foreach (var source in sources)
        {
            if (remaining == 0)
            {
                break;
            }
            remaining -= source.DrawPower(remaining, Result);
        }
        return amount - remaining;
    }
}
