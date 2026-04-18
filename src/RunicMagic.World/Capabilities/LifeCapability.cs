namespace RunicMagic.World.Capabilities;

public class LifeCapability(long maxHitPoints, long currentHitPoints)
{
    public long MaxHitPoints { get; } = maxHitPoints;
    public long CurrentHitPoints { get; set; } = currentHitPoints;
}
