namespace RunicMagic.World.Entities.ComplexAttributes;

public class StructuralIntegrityAttribute
{
    public long CurrentIntegrity { get; set; }
    public long MaxIntegrity { get; set; }

    public StructuralIntegrityAttribute(long currentIntegrity, long maxIntegrity)
    {
        CurrentIntegrity = currentIntegrity;
        MaxIntegrity = maxIntegrity;
    }
}
