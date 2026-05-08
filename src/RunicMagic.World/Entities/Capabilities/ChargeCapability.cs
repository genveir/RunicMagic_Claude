namespace RunicMagic.World.Entities.Capabilities;

public class ChargeCapability
{
    public long MaxCharge { get; }
    public long CurrentCharge { get; set; }

    public ChargeCapability(long maxCharge, long currentCharge)
    {
        MaxCharge = maxCharge;
        CurrentCharge = currentCharge;
    }
}
