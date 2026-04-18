namespace RunicMagic.World.Capabilities;

public class ChargeCapability(long maxCharge, long currentCharge)
{
    public long MaxCharge { get; } = maxCharge;
    public long CurrentCharge { get; set; } = currentCharge;
}
