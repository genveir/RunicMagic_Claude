using RunicMagic.Controller.Mappers;
using RunicMagic.Controller.Models;
using RunicMagic.World;
using RunicMagic.World.Geometry;

namespace RunicMagic.Controller.Services;

public class WorldRenderingService(WorldModel world, RayCastService rayCast)
{
    public IReadOnlyList<EntityRenderingModel> GetAllRenderingModels(EntityId? casterEntityId)
    {
        var entities = world.GetAll();

        var renderingModels = new List<EntityRenderingModel>();
        foreach (var entity in entities)
        {
            (int X, int Y)? pointingEnd = null;
            if (entity.PointingDirection.HasValue)
            {
                var castResult = rayCast.Cast(entity.Id, entity.X, entity.Y, entity.PointingDirection.Value);
                pointingEnd = (castResult.X, castResult.Y);
            }

            var mapped = EntityRenderingMapper.ToRenderingModel(entity, entity.Id == casterEntityId, pointingEnd);
            renderingModels.Add(mapped);
        }

        return renderingModels;
    }
}
