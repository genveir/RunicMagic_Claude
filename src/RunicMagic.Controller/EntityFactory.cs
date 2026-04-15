using RunicMagic.Database;
using RunicMagic.World;
using RunicMagic.World.Capabilities;

namespace RunicMagic.Controller;

public class EntityFactory(WorldModel world)
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
            Bounds = new Rectangle(data.X, data.Y, data.Width, data.Height),
            HasAgency = data.HasAgency,
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
                    var life = entity.Life!;
                    var given = Math.Min(amount, life.CurrentHitPoints);
                    life.CurrentHitPoints -= given;
                    return given;
                };
                break;

            case EntityType.ManaSource:
                entity.Reservoir = amount =>
                {
                    var charge = entity.Charge!;
                    var given = Math.Min(amount, charge.CurrentCharge);
                    charge.CurrentCharge -= given;
                    return given;
                };
                break;

            case EntityType.Object:
                break;
        }
    }
}
