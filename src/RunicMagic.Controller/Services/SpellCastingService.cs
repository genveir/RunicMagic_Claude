using RunicMagic.Controller.RuneParsing;
using RunicMagic.World;
using RunicMagic.World.Execution;

namespace RunicMagic.Controller.Services;

internal class SpellCastingService(WorldModel world, SpellExecutor spellExecutor)
{
    public IReadOnlyList<string> Cast(string input, EntityId? casterId)
    {
        var responseLines = new List<string>();

        if (casterId == null)
        {
            responseLines.Add("No caster selected.");
            return responseLines;
        }

        var casterEntity = world.Find(casterId.Value);
        if (casterEntity == null)
        {
            responseLines.Add("Caster not found in world.");
            return responseLines;
        }

        var (runeCount, parseResult) = SpellParser.Parse(input);

        if (!parseResult.Succeeded)
        {
            responseLines.Add(Describe(parseResult.Error));
            return responseLines;
        }

        var caster = new EntitySet([casterEntity]);
        var executor = new EntitySet([casterEntity]);

        var spellResult = spellExecutor.Execute(parseResult.Value, runeCount, caster, executor);

        foreach (var @event in spellResult.Events)
        {
            responseLines.Add(Describe(@event));
        }

        return responseLines;
    }

    private static string Describe(ParseEvent @event)
    {
        return @event switch
        {
            RanOutOfTokensEvent => "Spell is incomplete — ran out of runes.",
            UnexpectedTokenEvent e => $"Unexpected rune '{e.Token}' where {e.ExpectedType} was expected.",
            _ => @event.ToString()!
        };
    }

    private static string Describe(SpellEvent @event)
    {
        return @event switch
        {
            EntityPushedEvent e => $"{e.Entity.Label} pushed {e.DistanceMm}mm.",
            EntityPulledEvent e => $"{e.Entity.Label} pulled {e.DistanceMm}mm.",
            PowerDrawnEvent e => $"{e.Entity.Label} lost {e.Amount} power.",
            EntityDrainedEvent e => $"{e.Entity.Label} was drained.",
            PowerFilledEvent e => $"{e.Entity.Label} gained {e.Amount} power.",
            EntityFullEvent e => $"{e.Entity.Label} is full.",
            EffectNotFiredEvent e => $"Effect '{e.Effect}' did not fire: {e.Reason}.",
            ExecutorDisintegratedEvent => "The executor disintegrated.",
            SelectionCostNotMetEvent e => $"Selection failed: needed {e.Required} power, drew {e.Drawn}.",
            InscriptionReadEvent e => $"{e.Entity.Label}: {e.Text}",
            EntityRotatedEvent e => $"{e.Entity.Label} rotated {e.AngleDegrees} degrees.",
            DebugOutputEvent e => e.Text,
            _ => @event.ToString()!
        };
    }
}
