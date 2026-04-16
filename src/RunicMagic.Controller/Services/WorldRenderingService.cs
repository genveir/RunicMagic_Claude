using RunicMagic.Controller.Mappers;
using RunicMagic.Controller.Models;
using RunicMagic.World;

namespace RunicMagic.Controller.Services;

public class WorldRenderingService(WorldModel world)
{
    public IReadOnlyList<EntityRenderingModel> GetAllRenderingModels() =>
        [.. world.GetAll().Select(EntityRenderingMapper.ToRenderingModel)];
}
