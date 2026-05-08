using RunicMagic.Controller.EntityConstruction;
using RunicMagic.Database;
using RunicMagic.World;

namespace RunicMagic.Controller.Services;

public class WorldLoadingService
{
    private readonly WorldLoader loader;
    private readonly EntityFactory factory;
    private readonly WorldModel world;

    public WorldLoadingService(WorldLoader loader, EntityFactory factory, WorldModel world)
    {
        this.loader = loader;
        this.factory = factory;
        this.world = world;
    }

    public async Task LoadAsync()
    {
        var entities = await loader.LoadAsync();

        foreach (var entityData in entities)
        {
            world.Add(factory.Create(entityData));
        }
    }
}
