using RunicMagic.World.Capabilities;

namespace RunicMagic.World;

public class Entity(EntityId id, EntityType type, string label)
{
    public EntityId Id { get; } = id;
    public EntityType Type { get; } = type;
    public string Label { get; set; } = label;
    public Rectangle Bounds { get; set; }

    public LifeCapability? Life { get; set; }
    public ChargeCapability? Charge { get; set; }
    public bool HasAgency { get; set; }
    public int Weight { get; set; }
    public Func<Entity[]>? Scope { get; set; }
    public Func<int, int>? Reservoir { get; set; }
}
