namespace RunicMagic.World.Entities.Capabilities;

public class ChargeCapability
{
    public long CurrentCharge { get; set; }
    public long MaxCharge { get; }

    public ChargeCapability(long currentCharge, long maxCharge)
    {
        CurrentCharge = currentCharge;
        MaxCharge = maxCharge;

    }
}
