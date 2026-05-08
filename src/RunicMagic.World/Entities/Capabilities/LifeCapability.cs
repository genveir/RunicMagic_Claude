namespace RunicMagic.World.Entities.Capabilities;

public class LifeCapability
{
    public long MaxHitPoints { get; }
    public long CurrentHitPoints { get; set; }

    public LifeCapability(long maxHitPoints, long currentHitPoints)
    {
        MaxHitPoints = maxHitPoints;
        CurrentHitPoints = currentHitPoints;
    }
}
