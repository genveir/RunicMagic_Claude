using Microsoft.Extensions.Logging;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.Database;
using RunicMagic.World;
using RunicMagic.World.Capabilities;
using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Runes.RuneTypes;

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

        var entity = new Entity(new EntityId(data.Id), data.Label)
        {
            Location = new Location(data.X, data.Y),
            Angle = data.Angle,
            Width = data.Width,
            Height = data.Height,
            HasAgency = data.HasAgency,
            IsTranslucent = data.IsTranslucent,
            Weight = data.Weight,
        };

        if (data.MaxHitPoints.HasValue && data.CurrentHitPoints.HasValue)
            entity.Life = new LifeCapability(data.MaxHitPoints.Value, data.CurrentHitPoints.Value);

        if (data.MaxCharge.HasValue && data.CurrentCharge.HasValue)
            entity.Charge = new ChargeCapability(data.MaxCharge.Value, data.CurrentCharge.Value);

        WireDelegates(type, entity);
        ParseInscriptions(entity, data.InscriptionTexts);
        return entity;
    }

    private static void ParseInscriptions(Entity entity, string[]? inscriptionTexts)
    {
        if (inscriptionTexts is null || inscriptionTexts.Length == 0)
        {
            return;
        }

        var parsed = new List<IStatement>();
        foreach (var text in inscriptionTexts)
        {
            var statement = SpellParser.ParseAsStatement(text);
            if (statement is not null)
            {
                parsed.Add(statement);
            }
        }
        entity.ParsedInscriptions = [.. parsed];
        entity.RawInscriptions = inscriptionTexts;
    }

    private void WireDelegates(EntityType type, Entity entity)
    {
        entity.Scope = () => [.. world.GetEntitiesWithinDistance(entity, 500)];

        switch (type)
        {
            case EntityType.Creature:

                if (entity.Life is not null)
                {
                    entity.MaxReservoir = () => entity.Life.MaxHitPoints;
                    entity.CurrentReservoir = () => entity.Life.CurrentHitPoints;
                }

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

                if (entity.Charge is not null)
                {
                    entity.MaxReservoir = () => entity.Charge.MaxCharge;
                    entity.CurrentReservoir = () => entity.Charge.CurrentCharge;
                }

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
