using RunicMagic.World.Engine;

namespace RunicMagic.World.Execution;

internal record TemporalSpellContext(EntitySet Caster, EntitySet Executor, EngineAPI EngineAPI)
{
    public TemporalSpellContext(SpellContext spellContext)
        : this(spellContext.Caster, spellContext.Executor, spellContext.EngineAPI)
    {
    }

    public SpellContext ToSpellContext(IWorldEventTracker eventTracker)
    {
        return new SpellContext(Caster, Executor, EngineAPI, eventTracker);
    }
}
