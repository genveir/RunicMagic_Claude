using RunicMagic.Controller.Models;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.World;
using RunicMagic.World.Execution;

namespace RunicMagic.Controller.Services;

internal class SpellCastingService(WorldModel world, SpellExecutor spellExecutor)
{
    public void Cast(string input, EntityId? casterId, EventTracker eventTracker)
    {
        if (casterId == null)
        {
            eventTracker.Add(new NoCasterSelectedEvent());
            return;
        }

        var casterEntity = world.Find(casterId.Value);
        if (casterEntity == null)
        {
            eventTracker.Add(new CasterNotFoundEvent());
            return;
        }

        var (runeCount, parseResult) = SpellParser.Parse(input);

        if (!parseResult.Succeeded)
        {
            eventTracker.Add(parseResult.Error);
            return;
        }

        var caster = new EntitySet([casterEntity]);
        var executor = new EntitySet([casterEntity]);

        spellExecutor.Execute(parseResult.Value, eventTracker, runeCount, caster, executor);
    }
}
