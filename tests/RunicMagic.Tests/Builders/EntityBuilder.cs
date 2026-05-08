using RunicMagic.World.Entities;
using RunicMagic.World.Entities.AI;
using RunicMagic.World.Entities.Capabilities;
using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Tests.Builders;

internal class EntityBuilder
{
    private EntityId id = new EntityId(Guid.NewGuid());
    private string label = "Test Entity";
    private Location location = new Location(0, 0);
    private long width = 100;
    private long height = 100;
    private bool hasAgency = false;
    private long weight = 1000;
    private long strength = 0;
    private bool isTranslucent = false;
    private double angle = 0;
    private double dragCoefficient = 0.0;
    private double angularDragCoefficient = 0.0;
    private double groundFrictionCoefficient = 0.0;
    private double? groundContactRadius = 0.0;
    private StructuralIntegrityCapability structuralIntegrity = new StructuralIntegrityCapability(currentIntegrity: 1000, maxIntegrity: 1000);
    private AICapability aiCapability = new AICapability([]);

    private LifeCapability? life;
    private ChargeCapability? charge;
    private LocomotionCapability? locomotion;
    private Func<Entity[]>? scope;
    private ReservoirCapability? reservoir;
    private Direction? pointingDirection;
    private IndicateTarget? indicateTarget;
    private string[] rawInscriptions = [];
    private IStatement[] parsedInscriptions = [];

    public EntityBuilder() { }

    public EntityBuilder WithId(EntityId id)
    {
        this.id = id;
        return this;
    }

    public EntityBuilder WithLabel(string label)
    {
        this.label = label;
        return this;
    }

    public EntityBuilder WithLocation(long x, long y)
    {
        location = new Location(x, y);
        return this;
    }

    public EntityBuilder WithLocation(Location location)
    {
        this.location = location;
        return this;
    }

    public EntityBuilder WithSize(long width, long height)
    {
        this.width = width;
        this.height = height;
        return this;
    }

    public EntityBuilder WithWeight(long weight)
    {
        this.weight = weight;
        return this;
    }

    public EntityBuilder WithStrength(long strength)
    {
        this.strength = strength;
        return this;
    }

    public EntityBuilder WithAgency()
    {
        hasAgency = true;
        return this;
    }

    public EntityBuilder WithTranslucency()
    {
        isTranslucent = true;
        return this;
    }

    public EntityBuilder WithAngle(double angle)
    {
        this.angle = angle;
        return this;
    }

    public EntityBuilder WithDragCoefficient(double dragCoefficient)
    {
        this.dragCoefficient = dragCoefficient;
        return this;
    }

    public EntityBuilder WithAngularDragCoefficient(double angularDragCoefficient)
    {
        this.angularDragCoefficient = angularDragCoefficient;
        return this;
    }

    public EntityBuilder WithGroundFrictionCoefficient(double groundFrictionCoefficient)
    {
        this.groundFrictionCoefficient = groundFrictionCoefficient;
        return this;
    }

    public EntityBuilder WithGroundContactRadius(double groundContactRadius)
    {
        this.groundContactRadius = groundContactRadius;
        return this;
    }

    public EntityBuilder WithStructuralIntegrity(long current, long max)
    {
        structuralIntegrity = new StructuralIntegrityCapability(currentIntegrity: current, maxIntegrity: max);
        return this;
    }

    public EntityBuilder WithAICapability(AICapability aiCapability)
    {
        this.aiCapability = aiCapability;
        return this;
    }

    public EntityBuilder WithLocomotion(LocomotionCapability locomotion)
    {
        this.locomotion = locomotion;
        return this;
    }

    public EntityBuilder WithLife(long current, long max)
    {
        life = new LifeCapability(currentHitPoints: current, maxHitPoints: max);
        return this;
    }

    public EntityBuilder WithCharge(long current, long max)
    {
        charge = new ChargeCapability(currentCharge: current, maxCharge: max);
        return this;
    }

    public EntityBuilder WithReservoir(Func<long>? current = null, Func<long>? max = null, Func<long, ReservoirDraw>? draw = null, Func<long, ReservoirFill>? fill = null)
    {
        if (current == null) current = () => 1000000000;
        if (max == null) max = () => 1000000000;
        if (draw == null) draw = amount => new ReservoirDraw(amount, false);
        if (fill == null) fill = amount => new ReservoirFill(amount, false);

        reservoir = new ReservoirCapability(current: current, max: max, draw: draw, fill: fill);
        return this;
    }

    public EntityBuilder WithScope(Func<Entity[]> scope)
    {
        this.scope = scope;
        return this;
    }

    public EntityBuilder WithPointingDirection(Direction direction)
    {
        pointingDirection = direction;
        return this;
    }

    public EntityBuilder WithIndicateTarget(IndicateTarget target)
    {
        indicateTarget = target;
        return this;
    }

    public EntityBuilder WithInscriptions(string[] rawInscriptions, IStatement[] parsedInscriptions)
    {
        this.rawInscriptions = rawInscriptions;
        this.parsedInscriptions = parsedInscriptions;
        return this;
    }

    public Entity Build()
    {
        var w = (double)width;
        var h = (double)height;
        var groundContactRadius = this.groundContactRadius ?? Math.Sqrt((w * w + h * h) / 12.0);

        return new Entity(
            id: id,
            label: label,
            location: location,
            width: width,
            height: height,
            hasAgency: hasAgency,
            weight: weight,
            strength: strength,
            isTranslucent: isTranslucent,
            angle: angle,
            structuralIntegrity: structuralIntegrity,
            aiCapability: aiCapability,
            dragCoefficient: dragCoefficient,
            angularDragCoefficient: angularDragCoefficient,
            groundFrictionCoefficient: groundFrictionCoefficient,
            groundContactRadius: groundContactRadius)
        {
            Life = life,
            Charge = charge,
            Locomotion = locomotion,
            Scope = scope,
            Reservoir = reservoir,
            PointingDirection = pointingDirection,
            IndicateTarget = indicateTarget,
            RawInscriptions = rawInscriptions,
            ParsedInscriptions = parsedInscriptions,
        };
    }
}
