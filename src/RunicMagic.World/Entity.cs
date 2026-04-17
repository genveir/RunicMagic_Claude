using RunicMagic.World.Capabilities;
using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;

namespace RunicMagic.World;

public class Entity(EntityId id, EntityType type, string label)
{
    public EntityId Id { get; } = id;
    public EntityType Type { get; } = type;
    public string Label { get; set; } = label;
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public LifeCapability? Life { get; set; }
    public ChargeCapability? Charge { get; set; }
    public bool HasAgency { get; set; }
    public int Weight { get; set; }
    public Func<Entity[]>? Scope { get; set; }
    public Func<int, ReservoirDraw>? Reservoir { get; set; }
    public Direction? PointingDirection { get; set; }
}
