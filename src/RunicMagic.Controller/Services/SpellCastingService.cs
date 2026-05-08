using RunicMagic.Controller.RuneParsing;
using RunicMagic.World.Entities;
using RunicMagic.World.Execution;

namespace RunicMagic.Controller.Services;

internal class SpellCastingService
{
    private readonly SpellExecutor spellExecutor;

    public SpellCastingService(SpellExecutor spellExecutor)
    {
        this.spellExecutor = spellExecutor;
    }

    public void Cast(string input, Entity caster, EventTracker eventTracker)
    {
        var (runeCount, parseResult) = SpellParser.Parse(input);

        if (!parseResult.Succeeded)
        {
            eventTracker.Add(parseResult.Error);
            return;
        }

        var casterSet = new EntitySet([caster]);
        var executor = new EntitySet([caster]);

        spellExecutor.Execute(parseResult.Value, eventTracker, runeCount, casterSet, executor);
    }
}
