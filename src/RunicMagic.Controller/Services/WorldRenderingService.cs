using RunicMagic.Controller.Mappers;
using RunicMagic.Controller.Models;
using RunicMagic.World;

namespace RunicMagic.Controller.Services;

public class WorldRenderingService(WorldModel world)
{
    public IReadOnlyList<EntityRenderingModel> GetAllRenderingModels()
    {
        var entities = world.GetAll();

        var renderingModels = new List<EntityRenderingModel>();
        foreach (var entity in entities)
        {
            var mapped = EntityRenderingMapper.ToRenderingModel(entity);
            renderingModels.Add(mapped);
        }

        return renderingModels;
    }
}
