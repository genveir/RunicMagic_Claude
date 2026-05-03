namespace RunicMagic.World.Entities.Capabilities;

public class LifeCapability(long maxHitPoints, long currentHitPoints)
{
    public long MaxHitPoints { get; } = maxHitPoints;
    public long CurrentHitPoints { get; set; } = currentHitPoints;
}
