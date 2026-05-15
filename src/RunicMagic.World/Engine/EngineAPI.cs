namespace RunicMagic.World.Engine;

public class EngineAPI
{
    public EntitySetSelectService EntitySetSelectService { get; }

    public EngineAPI(EntitySetSelectService entitySetSelectService)
    {
        EntitySetSelectService = entitySetSelectService;
    }
}
