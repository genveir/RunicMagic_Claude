namespace RunicMagic.World.Entities.Capabilities;

public class StructuralIntegrityCapability
{
    public long CurrentIntegrity { get; set; }
    public long MaxIntegrity { get; }

    public StructuralIntegrityCapability(long currentIntegrity, long maxIntegrity)
    {
        CurrentIntegrity = currentIntegrity;
        MaxIntegrity = maxIntegrity;
    }
}
