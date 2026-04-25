using Microsoft.Extensions.Logging;
using RunicMagic.World;
using RunicMagic.World.Capabilities;

namespace RunicMagic.Controller.EntityConstruction;

internal static class ManaSourceWiring
{
    public static void WireDelegates(EntityType entityType, Entity entity, ILogger logger)
    {
        if (entity.Charge == null)
        {
            return;
        }

        entity.Reservoir = new ReservoirCapability(
            max: () => entity.Charge.MaxCharge,
            current: () => entity.Charge.CurrentCharge,
            draw: amount => Draw(logger, entity, amount),
            fill: amount => Fill(logger, entity, amount));
    }

    private static ReservoirDraw Draw(ILogger logger, Entity entity, long amount)
    {
        if (entity.Charge is null)
        {
            logger.LogWarning("Mana Source {EntityId} ({Label}) has no ChargeCapability — returning 0 power", entity.Id, entity.Label);
            return new ReservoirDraw(0, false);
        }
        var given = Math.Min(amount, entity.Charge.CurrentCharge);
        entity.Charge.CurrentCharge -= given;
        return new ReservoirDraw(given, entity.Charge.CurrentCharge == 0 && given > 0);
    }

    private static ReservoirFill Fill(ILogger logger, Entity entity, long amount)
    {
        if (entity.Charge is null)
        {
            logger.LogWarning("Mana Source {EntityId} ({Label}) has no ChargeCapability — cannot fill", entity.Id, entity.Label);
            return new ReservoirFill(0, false);
        }
        var accepted = Math.Min(amount, entity.Charge.MaxCharge - entity.Charge.CurrentCharge);
        entity.Charge.CurrentCharge += accepted;
        return new ReservoirFill(accepted, entity.Charge.CurrentCharge == entity.Charge.MaxCharge);
    }
}
