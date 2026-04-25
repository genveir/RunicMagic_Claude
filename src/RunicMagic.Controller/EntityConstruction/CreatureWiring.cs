using Microsoft.Extensions.Logging;
using RunicMagic.World;
using RunicMagic.World.Capabilities;

namespace RunicMagic.Controller.EntityConstruction;

internal static class CreatureWiring
{
    public static void WireDelegates(EntityType entityType, Entity entity, ILogger logger)
    {
        if (entity.Life == null)
        {
            return;
        }

        entity.Reservoir = new ReservoirCapability(
            max: () => entity.Life.MaxHitPoints,
            current: () => entity.Life.CurrentHitPoints,
            draw: amount => Draw(logger, entity, amount),
            fill: amount => Fill(logger, entity, amount)
        );
    }

    private static ReservoirDraw Draw(ILogger logger, Entity entity, long amount)
    {
        if (entity.Life is null)
        {
            logger.LogWarning("Creature {EntityId} ({Label}) has no LifeCapability — returning 0 power", entity.Id, entity.Label);
            return new ReservoirDraw(0, false);
        }
        var given = Math.Min(amount, entity.Life.CurrentHitPoints);
        entity.Life.CurrentHitPoints -= given;
        return new ReservoirDraw(given, entity.Life.CurrentHitPoints == 0 && given > 0);
    }

    private static ReservoirFill Fill(ILogger logger, Entity entity, long amount)
    {
        if (entity.Life is null)
        {
            logger.LogWarning("Creature {EntityId} ({Label}) has no LifeCapability — cannot fill", entity.Id, entity.Label);
            return new ReservoirFill(0, false);
        }
        var accepted = Math.Min(amount, entity.Life.MaxHitPoints - entity.Life.CurrentHitPoints);
        entity.Life.CurrentHitPoints += accepted;
        return new ReservoirFill(accepted, entity.Life.CurrentHitPoints == entity.Life.MaxHitPoints);
    }
}
