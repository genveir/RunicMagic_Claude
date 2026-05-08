using RunicMagic.Controller.Models;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.World.Execution;

namespace RunicMagic.Controller.Services;

internal static class EventDescriber
{
    internal static string Describe(WorldEvent worldEvent)
    {
        var description = worldEvent switch
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
            _ => worldEvent.ToString()!
        };
        return description;
    }

    internal static string Describe(ControllerEvent controllerEvent)
    {
        var description = controllerEvent switch
        {
            NoCasterSelectedEvent => "No caster selected.",
            CasterNotFoundEvent => "Caster not found in world.",
            CasterDeadEvent => "Caster is dead.",
            CasterDestroyedEvent => "Caster is destroyed.",
            NoEntitiesWithAgencyFoundEvent => "No entities with agency found at that position.",
            MultipleEntitiesWithAgencyFoundEvent => "Multiple entities with agency found at that position — unable to resolve a caster.",
            CasterSetEvent e => $"Caster set to {e.Entity.Label}.",
            CasterMovedEvent => "Caster moved.",
            PointingDirectionSetEvent => "Pointing direction set.",
            NothingToIndicateEvent => "Nothing to indicate at that position.",
            IndicatingEvent e => $"Indicating {e.Entity.Label}.",
            IndicateTargetBlockedEvent => "Cannot reach that — something is in the way.",
            IndicateTargetOutOfReachEvent e => $"{e.Entity.Label} is out of reach.",
            _ => controllerEvent.ToString()!
        };
        return description;
    }

    internal static string Describe(ParseEvent parseEvent)
    {
        var description = parseEvent switch
        {
            RanOutOfTokensEvent => "Spell is incomplete — ran out of runes.",
            UnexpectedTokenEvent e => $"Unexpected rune '{e.Token}' where {e.ExpectedType} was expected.",
            _ => parseEvent.ToString()!
        };
        return description;
    }
}
