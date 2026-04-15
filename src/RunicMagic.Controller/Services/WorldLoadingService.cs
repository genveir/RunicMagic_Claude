using RunicMagic.Database;
using RunicMagic.World;

namespace RunicMagic.Controller.Services;

public class WorldLoadingService(WorldLoader loader, EntityFactory factory, WorldModel world)
{
    public async Task LoadAsync()
    {
        foreach (var data in await loader.LoadAsync())
            world.Add(factory.Create(data));
    }
}
