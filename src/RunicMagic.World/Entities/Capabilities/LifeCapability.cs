namespace RunicMagic.World.Entities.Capabilities;

public class LifeCapability
{
    public long CurrentHitPoints { get; set; }
    public long MaxHitPoints { get; }

    public LifeCapability(long currentHitPoints, long maxHitPoints)
    {
        CurrentHitPoints = currentHitPoints;
        MaxHitPoints = maxHitPoints;
    }
}
