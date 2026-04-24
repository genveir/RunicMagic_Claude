using RunicMagic.World.Capabilities;
using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World;

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
        StructuralIntegrityCapability structuralIntegrity)
    {
        Id = id;
        Label = label;
        Location = location;
        Width = width;
        Height = height;
        HasAgency = hasAgency;
        Weight = weight;
        IsTranslucent = isTranslucent;
        Angle = angle;
        StructuralIntegrity = structuralIntegrity;
    }

    public EntityId Id { get; }

    public string Label { get; set; }
    public Location Location { get; set; }
    public long Width { get; set; }
    public long Height { get; set; }
    public double Angle { get; set; }

    public bool HasAgency { get; set; }
    public bool IsTranslucent { get; set; }
    public long Weight { get; set; }

    public Func<Entity[]>? Scope { get; set; }

    public LifeCapability? Life { get; set; }
    public ChargeCapability? Charge { get; set; }
    public StructuralIntegrityCapability StructuralIntegrity { get; set; }

    public Func<long, ReservoirDraw>? Reservoir { get; set; }
    public Func<long>? MaxReservoir { get; set; }
    public Func<long>? CurrentReservoir { get; set; }

    public Direction? PointingDirection { get; set; }
    public IndicateTarget? IndicateTarget { get; set; }

    public string[] RawInscriptions { get; set; } = [];
    public IStatement[] ParsedInscriptions { get; set; } = [];

    public override string ToString()
    {
        return $"Entity {Label}";
    }
}
