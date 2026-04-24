namespace RunicMagic.World.Capabilities;

public class StructuralIntegrityCapability(long maxIntegrity, long currentIntegrity)
{
    public long MaxIntegrity { get; } = maxIntegrity;
    public long CurrentIntegrity { get; set; } = currentIntegrity;
}
