using Microsoft.Extensions.Logging;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.Database;
using RunicMagic.World;
using RunicMagic.World.AI;
using RunicMagic.World.Capabilities;
using RunicMagic.World.Geometry;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.EntityConstruction;

public class EntityFactory(WorldModel world, ILogger<EntityFactory> logger)
{
    public Entity Create(EntityData entityData)
    {
        var type = entityData.TypeId switch
        {
            1 => EntityType.Creature,
            2 => EntityType.ManaSource,
            3 => EntityType.Object,
            _ => throw new ArgumentException($"Unknown entity type ID: {entityData.TypeId}")
        };

        var aiCapability = SetupAI(entityData.AIData);

        var entity = new Entity(
            id: new EntityId(entityData.Id),
            label: entityData.Label,
            location: new Location(entityData.X, entityData.Y),
            width: entityData.Width,
            height: entityData.Height,
            hasAgency: entityData.HasAgency,
            weight: entityData.Weight,
            isTranslucent: entityData.IsTranslucent,
            angle: entityData.Angle,
            structuralIntegrity: new StructuralIntegrityCapability(entityData.MaxStructuralIntegrity, entityData.CurrentStructuralIntegrity),
            aiCapability: aiCapability);

        if (entityData.MaxHitPoints.HasValue && entityData.CurrentHitPoints.HasValue)
            entity.Life = new LifeCapability(entityData.MaxHitPoints.Value, entityData.CurrentHitPoints.Value);

        if (entityData.MaxCharge.HasValue && entityData.CurrentCharge.HasValue)
            entity.Charge = new ChargeCapability(entityData.MaxCharge.Value, entityData.CurrentCharge.Value);

        WireDelegates(type, entity);
        ParseInscriptions(entity, entityData.InscriptionTexts);
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
        entity.Scope = () => world.GetEntitiesWithinDistance(entity, distance: 500).ToArray();

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

    private AICapability SetupAI(AIData? aiData)
    {
        return new AICapability([]);
    }
}
