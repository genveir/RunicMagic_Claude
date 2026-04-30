namespace RunicMagic.World.Execution;

internal record TemporalSpellContext(EntitySet Caster, EntitySet Executor, WorldModel World)
{
    public TemporalSpellContext(SpellContext spellContext)
        : this(spellContext.Caster, spellContext.Executor, spellContext.World)
    {
    }

    public SpellContext ToSpellContext(IWorldEventTracker eventTracker)
    {
        return new SpellContext(Caster, Executor, World, eventTracker);
    }
}
