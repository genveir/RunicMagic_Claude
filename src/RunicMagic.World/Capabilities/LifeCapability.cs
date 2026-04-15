namespace RunicMagic.World.Capabilities;

public class LifeCapability(int maxHitPoints, int currentHitPoints)
{
    public int MaxHitPoints { get; } = maxHitPoints;
    public int CurrentHitPoints { get; set; } = currentHitPoints;
}
