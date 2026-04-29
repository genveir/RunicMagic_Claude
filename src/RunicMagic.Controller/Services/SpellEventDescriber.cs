using RunicMagic.World.Execution;

namespace RunicMagic.Controller.Services;

internal static class SpellEventDescriber
{
    internal static string Describe(SpellEvent @event)
    {
        var description = @event switch
        {
            EntityPushedEvent e => $"{e.Entity.Label} pushed {e.DistanceMm}mm.",
            EntityPulledEvent e => $"{e.Entity.Label} pulled {e.DistanceMm}mm.",
            PowerDrawnEvent e => $"{e.Entity.Label} lost {e.Amount} power.",
            EntityDrainedEvent e => $"{e.Entity.Label} was drained.",
            PowerFilledEvent e => $"{e.Entity.Label} gained {e.Amount} power.",
            EntityFullEvent e => $"{e.Entity.Label} is full.",
            EffectNotFiredEvent e => $"Effect '{e.Effect}' did not fire: {e.Reason}.",
            EntityDisintegratedEvent e => $"{e.Entity.Label} disintegrated.",
            SelectionCostNotMetEvent e => $"Selection failed: needed {e.Required} power, drew {e.Drawn}.",
            InscriptionReadEvent e => $"{e.Entity.Label}: {e.Text}",
            EntityRotatedEvent e => $"{e.Entity.Label} rotated {e.AngleDegrees} degrees.",
            DebugOutputEvent e => e.Text,
            _ => @event.ToString()!
        };
        return description;
    }
}
