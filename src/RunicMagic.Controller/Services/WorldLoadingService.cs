using RunicMagic.Controller.EntityConstruction;
using RunicMagic.Database;
using RunicMagic.World;

namespace RunicMagic.Controller.Services;

public class WorldLoadingService(WorldLoader loader, EntityFactory factory, WorldModel world)
{
    public async Task LoadAsync()
    {
        var entities = await loader.LoadAsync();

        foreach (var entityData in entities)
        {
            world.Add(factory.Create(entityData));
        }
    }
}
