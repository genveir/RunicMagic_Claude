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
            WorldCoordinate? pointingEnd = null;
            if (entity.PointingDirection.HasValue)
            {
                var castResult = rayCast.Cast(entity.Id, entity.Location, entity.PointingDirection.Value);
                pointingEnd = WorldCoordinate.FromLocation(castResult.LocationOfIntersect);
            }

            WorldCoordinate? indicateEnd = null;
            if (entity.IndicateTarget?.Direction != null)
            {
                var direction = entity.IndicateTarget.Direction.Value;
                var castResult = rayCast.Cast(entity.Id, entity.Location, direction, skipTranslucent: false);
                var dist = castResult.LocationOfIntersect.GetDistanceTo(entity.Location);

                var capped = Math.Min(dist, 1000);
                var endX = (long)Math.Round(entity.Location.X + direction.X * capped);
                var endY = (long)Math.Round(entity.Location.Y + direction.Y * capped);

                indicateEnd = new WorldCoordinate(endX, endY);
            }

            var isIndicateTarget = entity.Id == indicateTargetId;
            var mapped = EntityRenderingMapper.ToRenderingModel(entity, entity.Id == casterEntityId, pointingEnd, isIndicateTarget, indicateEnd);
            renderingModels.Add(mapped);
        }

        return renderingModels;
    }
}
