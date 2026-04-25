using Microsoft.Extensions.Logging;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.Database;
using RunicMagic.World;
using RunicMagic.World.Capabilities;
using RunicMagic.World.Geometry;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.EntityConstruction;

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

        var entity = new Entity(
            id: new EntityId(data.Id),
            label: data.Label,
            location: new Location(data.X, data.Y),
            width: data.Width,
            height: data.Height,
            hasAgency: data.HasAgency,
            weight: data.Weight,
            isTranslucent: data.IsTranslucent,
            angle: data.Angle,
            structuralIntegrity: new StructuralIntegrityCapability(data.MaxStructuralIntegrity, data.CurrentStructuralIntegrity));

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
        entity.Scope = () => GetEntitiesWithinDistance(entity, distance: 500);

        switch (type)
        {
            case EntityType.Creature:
                CreatureWiring.WireDelegates(type, entity, logger);
                break;

            case EntityType.ManaSource:
                ManaSourceWiring.WireDelegates(type, entity, logger);
                break;

            case EntityType.Object:
                break;
        }
    }

    private Entity[] GetEntitiesWithinDistance(Entity entity, long distance)
    {
        return world.GetEntitiesWithinDistance(entity, distance).ToArray();
    }
}
