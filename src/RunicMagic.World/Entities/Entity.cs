using RunicMagic.World.Entities.AI;
using RunicMagic.World.Entities.Capabilities;
using RunicMagic.World.Entities.ComplexAttributes;
using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Motion.Simulated;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Entities;

public class Entity
{
    public Entity(
        EntityId id,
        string label,
        Location location,
        long width,
        long height,
        bool hasAgency,
        long weight,
        bool isTranslucent,
        double angle,
        StructuralIntegrityAttribute structuralIntegrity,
        AICapability aiCapability,
        double dragCoefficient,
        double angularDragCoefficient,
        double groundFrictionCoefficient,
        double groundContactRadius)
    {
        Id = id;
        Label = label;
        Location = location;
        Width = width;
        Height = height;
        HasAgency = hasAgency;
        Weight = weight;
        IsTranslucent = isTranslucent;
        FacingAngle = angle;
        StructuralIntegrity = structuralIntegrity;
        AI = aiCapability;
        DragCoefficient = dragCoefficient;
        AngularDragCoefficient = angularDragCoefficient;
        GroundFrictionCoefficient = groundFrictionCoefficient;
        GroundContactRadius = groundContactRadius;

        Velocity = new VelocityVector(0, 0, 0);
    }

    public EntityId Id { get; }

    public string Label { get; set; }
    public Location Location { get; set; }
    public long Width { get; set; }
    public long Height { get; set; }
    public double FacingAngle { get; set; }

    public Rectangle Bounds => new(
        Location: Location,
        Width: Width,
        Height: Height,
        Angle: FacingAngle);

    public bool HasAgency { get; set; }
    public bool IsTranslucent { get; set; }

    public long Weight { get; set; }
    public double DragCoefficient { get; set; }
    public double AngularDragCoefficient { get; set; }
    public double GroundFrictionCoefficient { get; set; }
    public double GroundContactRadius { get; set; }
    public bool IsGrounded { get; set; } = true;

    public Func<Entity[]>? Scope { get; set; }

    public ReservoirCapability? Reservoir { get; set; }
    public LifeCapability? Life { get; set; }
    public ChargeCapability? Charge { get; set; }
    public LocomotionCapability? Locomotion { get; set; }
    public StrengthCapability? Strength { get; set; }
    public AICapability AI { get; set; }

    public StructuralIntegrityAttribute StructuralIntegrity { get; set; }

    public Direction? PointingDirection { get; set; }
    public IndicateTarget? IndicateTarget { get; set; }

    public string[] RawInscriptions { get; set; } = [];
    public IStatement[] ParsedInscriptions { get; set; } = [];

    public bool IsUnderEngineMotion { get; set; } = false;

    public VelocityVector Velocity { get; set; }
    public List<ForceVector> PendingImpulses { get; } = [];

    public override string ToString()
    {
        return $"Entity {Label}";
    }
}
