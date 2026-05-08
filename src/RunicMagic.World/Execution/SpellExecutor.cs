using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Execution;

public class SpellExecutor
{
    private readonly WorldModel world;

    public SpellExecutor(WorldModel world)
    {
        this.world = world;
    }

    public void Execute(IExecutableStatement spell, IWorldEventTracker eventTracker, long runeCount, EntitySet caster, EntitySet executor)
    {
        var context = new SpellContext(caster, executor, world, eventTracker);

        var toDraw = runeCount * 1000000;

        var evalDrawn = context.DrawPower(toDraw);

        if (evalDrawn < toDraw)
        {
            foreach (var entity in executor.Entities)
            {
                eventTracker.Add(new EntityDisintegratedEvent(entity));
                world.Remove(entity.Id);
            }
            return;
        }

        spell.Execute(context);
    }
}
