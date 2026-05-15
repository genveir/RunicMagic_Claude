using RunicMagic.World.Engine;
using RunicMagic.World.Entities;

namespace RunicMagic.World.Execution;

public class SpellContext
{
    private readonly Stack<EntitySet> sourceStack = new();
    private readonly Stack<EntitySelection> resolutionStack = new();

    public EntitySet Caster { get; }
    public EntitySet Executor { get; }
    public EngineAPI EngineAPI { get; }
    public IWorldEventTracker EventTracker { get; internal set; }

    public SpellContext(EntitySet caster, EntitySet executor, EngineAPI engineAPI, IWorldEventTracker eventTracker)
    {
        Caster = caster;
        Executor = executor;
        EngineAPI = engineAPI;
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

    public EntitySelection? EntityResolutionCount =>
        resolutionStack.TryPeek(out var top) ? top : null;

    public void OpenResolutionWindow()
    {
        resolutionStack.Push(new EntitySelection(new HashSet<EntityId>(), 0));
    }

    public void CloseResolutionWindow()
    {
        resolutionStack.Pop();
    }

    public SpellContext ForkWithNewExecutor(EntitySet newExecutor)
    {
        var forked = new SpellContext(Caster, newExecutor, EngineAPI, EventTracker);
        foreach (var source in sourceStack.Reverse())
        {
            forked.sourceStack.Push(source);
        }
        foreach (var selection in resolutionStack.Reverse())
        {
            forked.resolutionStack.Push(selection with { EntityIds = [.. selection.EntityIds] });
        }
        return forked;
    }

    public long DrawPower(long amount)
    {
        var executorUnionScopeSelection = EngineAPI.EntitySetSelectService.GetUnionScope(Executor.Entities);
        var executorUnionScope = new EntitySet(executorUnionScopeSelection.Entities);

        var casterUnionScopeSelection = EngineAPI.EntitySetSelectService.GetUnionScope(Caster.Entities);
        var casterUnionScope = new EntitySet(casterUnionScopeSelection.Entities);

        var remaining = amount;
        var sources = sourceStack.Concat([executorUnionScope, Executor, casterUnionScope, Caster]);
        foreach (var source in sources)
        {
            if (remaining == 0)
            {
                break;
            }
            remaining -= EngineAPI.PowerService.DrawPower(source, remaining, EventTracker);
        }
        return amount - remaining;
    }
}
