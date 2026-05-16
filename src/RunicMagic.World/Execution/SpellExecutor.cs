using RunicMagic.World.Engine;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Execution;

public class SpellExecutor
{
    private readonly WorldModel world;
    private readonly EngineAPI engineAPI;

    public SpellExecutor(WorldModel world, EngineAPI engineAPI)
    {
        this.world = world;
        this.engineAPI = engineAPI;
    }

    public void Execute(IExecutableStatement spell, IWorldEventTracker eventTracker, long runeCount, EntitySet caster, EntitySet executor)
    {
        var context = new SpellContext(caster, executor, engineAPI, eventTracker);

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
