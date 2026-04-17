using Microsoft.Extensions.Logging;
using RunicMagic.Database;
using RunicMagic.World;
using RunicMagic.World.Capabilities;
using RunicMagic.World.Execution;

namespace RunicMagic.Controller;

public class EntityFactory(WorldModel world, ILogger<EntityFactory> logger)
{
    public Entity Create(EntityData data)
    {
        var type = data.TypeId switch
        {
            1 => EntityType.Creature,
            2 => EntityType.ManaSource,
            3 => EntityType.Object,
            _ => throw new ArgumentException($"Unknown entity type ID: {data.TypeId}")
        };

        var entity = new Entity(new EntityId(data.Id), type, data.Label)
        {
            X = data.X,
            Y = data.Y,
            Width = data.Width,
            Height = data.Height,
            HasAgency = data.HasAgency,
            Weight = data.Weight,
        };

        if (data.MaxHitPoints.HasValue && data.CurrentHitPoints.HasValue)
            entity.Life = new LifeCapability(data.MaxHitPoints.Value, data.CurrentHitPoints.Value);

        if (data.MaxCharge.HasValue && data.CurrentCharge.HasValue)
            entity.Charge = new ChargeCapability(data.MaxCharge.Value, data.CurrentCharge.Value);

        WireDelegates(entity);
        return entity;
    }

    private void WireDelegates(Entity entity)
    {
        switch (entity.Type)
        {
            case EntityType.Creature:
                entity.Scope = () => [.. world.GetTouchingEntities(entity)];
                entity.Reservoir = amount =>
                {
                    if (entity.Life is null)
                    {
                        logger.LogWarning("Creature {EntityId} ({Label}) has no LifeCapability — returning 0 power", entity.Id, entity.Label);
                        return new ReservoirDraw(0, false);
                    }
                    var given = Math.Min(amount, entity.Life.CurrentHitPoints);
                    entity.Life.CurrentHitPoints -= given;
                    return new ReservoirDraw(given, entity.Life.CurrentHitPoints == 0 && given > 0);
                };
                break;

            case EntityType.ManaSource:
                entity.Reservoir = amount =>
                {
                    if (entity.Charge is null)
                    {
                        logger.LogWarning("ManaSource {EntityId} ({Label}) has no ChargeCapability — returning 0 power", entity.Id, entity.Label);
                        return new ReservoirDraw(0, false);
                    }
                    var given = Math.Min(amount, entity.Charge.CurrentCharge);
                    entity.Charge.CurrentCharge -= given;
                    return new ReservoirDraw(given, entity.Charge.CurrentCharge == 0 && given > 0);
                };
                break;

            case EntityType.Object:
                break;
        }
    }
}
