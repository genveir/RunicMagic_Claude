namespace RunicMagic.World.Capabilities;

public class ChargeCapability(int maxCharge, int currentCharge)
{
    public int MaxCharge { get; } = maxCharge;
    public int CurrentCharge { get; set; } = currentCharge;
}
