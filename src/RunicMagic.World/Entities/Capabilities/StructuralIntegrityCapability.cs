namespace RunicMagic.World.Entities.Capabilities;

public class StructuralIntegrityCapability
{
    public long MaxIntegrity { get; }
    public long CurrentIntegrity { get; set; }

    public StructuralIntegrityCapability(long maxIntegrity, long currentIntegrity)
    {
        MaxIntegrity = maxIntegrity;
        CurrentIntegrity = currentIntegrity;
    }
}
