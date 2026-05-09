using RunicMagic.Controller.RuneParsing;
using RunicMagic.Database;
using RunicMagic.World;
using RunicMagic.World.Entities;
using RunicMagic.World.Entities.AI;
using RunicMagic.World.Entities.Capabilities;
using RunicMagic.World.Geometry;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.EntityConstruction;

public class EntityFactory
{
    private readonly WorldModel world;
    private readonly ILogger<EntityFactory> logger;

    public EntityFactory(WorldModel world, ILogger<EntityFactory> logger)
    {
        this.world = world;
        this.logger = logger;
    }

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

        List<Leg>? legs = null;
        double groundContactRadius;
        if (entityData.LocomotionEfficiency.HasValue)
        {
            legs = new List<Leg>
            {
                new Leg("Left", lateralOffset: -(entityData.Width / 4), forwardOffset: 0),
                new Leg("Right", lateralOffset: entityData.Width / 4, forwardOffset: 0)
            };
            var sumSq = 0.0;
            foreach (var leg in legs)
            {
                sumSq += (double)leg.LateralOffset * leg.LateralOffset + (double)leg.ForwardOffset * leg.ForwardOffset;
            }
            groundContactRadius = Math.Sqrt(sumSq / legs.Count);
        }
        else
        {
            var w = (double)entityData.Width;
            var h = (double)entityData.Height;
            groundContactRadius = Math.Sqrt((w * w + h * h) / 12.0);
        }

        var entity = new Entity(
            id: new EntityId(entityData.Id),
            label: entityData.Label,
            location: new Location(entityData.X, entityData.Y),
            width: entityData.Width,
            height: entityData.Height,
            hasAgency: entityData.HasAgency,
            weight: entityData.Weight,
            strength: entityData.Strength,
            isTranslucent: entityData.IsTranslucent,
            angle: entityData.Angle,
            structuralIntegrity: new StructuralIntegrityCapability(currentIntegrity: entityData.CurrentStructuralIntegrity, maxIntegrity: entityData.MaxStructuralIntegrity),
            aiCapability: aiCapability,
            dragCoefficient: entityData.DragCoefficient,
            angularDragCoefficient: entityData.AngularDragCoefficient,
            groundFrictionCoefficient: entityData.GroundFrictionCoefficient,
            groundContactRadius: groundContactRadius);

        if (entityData.MaxHitPoints.HasValue && entityData.CurrentHitPoints.HasValue)
            entity.Life = new LifeCapability(currentHitPoints: entityData.CurrentHitPoints.Value, maxHitPoints: entityData.MaxHitPoints.Value);

        if (entityData.MaxCharge.HasValue && entityData.CurrentCharge.HasValue)
            entity.Charge = new ChargeCapability(currentCharge: entityData.CurrentCharge.Value, maxCharge: entityData.MaxCharge.Value);

        if (legs != null)
        {
            entity.Locomotion = new LocomotionCapability(legs, locomotionEfficiency: entityData.LocomotionEfficiency!.Value);
        }

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
        var behaviors = new List<IAIBehavior>();

        if (aiData?.PatrolBehaviors != null)
        {
            foreach (var patrolData in aiData.PatrolBehaviors)
            {
                var waypoints = patrolData.Waypoints
                    .Select(w => new PatrolWaypoint(new Location(w.X, w.Y), w.WaitTicks))
                    .ToList();
                behaviors.Add(new PatrolAIBehavior(waypoints, patrolData.Speed));
            }
        }

        return new AICapability(behaviors);
    }
}
