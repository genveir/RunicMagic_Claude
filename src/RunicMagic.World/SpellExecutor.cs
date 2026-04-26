using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World;

public class SpellExecutor
{
    private readonly WorldModel _world;

    public SpellExecutor(WorldModel world)
    {
        _world = world;
    }

    public SpellResult Execute(IExecutableStatement spell, long runeCount, EntitySet caster, EntitySet executor)
    {
        var result = new SpellResult();
        var context = new SpellContext(caster, executor, _world, result);

        var evalDrawn = context.DrawPower(runeCount);

        if (evalDrawn < runeCount)
        {
            foreach (var entity in executor.Entities)
            {
                result.Add(new EntityDisintegratedEvent(entity));
                _world.Remove(entity.Id);
            }
            return result;
        }

        spell.Execute(context);
        return result;
    }
}
