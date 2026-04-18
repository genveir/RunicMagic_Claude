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

        var indicateTargetId = casterEntityId.HasValue
            ? world.Find(casterEntityId.Value)?.IndicateTarget?.EntityId
            : null;

        var renderingModels = new List<EntityRenderingModel>();
        foreach (var entity in entities)
        {
            (int X, int Y)? pointingEnd = null;
            if (entity.PointingDirection.HasValue)
            {
                var castResult = rayCast.Cast(entity.Id, entity.X, entity.Y, entity.PointingDirection.Value);
                pointingEnd = (castResult.X, castResult.Y);
            }

            (int X, int Y)? indicateEnd = null;
            if (entity.IndicateTarget?.Direction != null)
            {
                var direction = entity.IndicateTarget.Direction.Value;
                var castResult = rayCast.Cast(entity.Id, entity.X, entity.Y, direction, skipTranslucent: false);
                var dx = castResult.X - entity.X;
                var dy = castResult.Y - entity.Y;
                var dist = Math.Sqrt(dx * dx + dy * dy);
                var capped = Math.Min(dist, 1000);
                var endX = (int)Math.Round(entity.X + direction.X * capped);
                var endY = (int)Math.Round(entity.Y + direction.Y * capped);
                indicateEnd = (endX, endY);
            }

            var isIndicateTarget = entity.Id == indicateTargetId;
            var mapped = EntityRenderingMapper.ToRenderingModel(entity, entity.Id == casterEntityId, pointingEnd, isIndicateTarget, indicateEnd);
            renderingModels.Add(mapped);
        }

        return renderingModels;
    }
}
