using RunicMagic.Controller.Mappers;
using RunicMagic.View.Abstractions;
using RunicMagic.View.Models;
using RunicMagic.World;
using RunicMagic.World.Entities;
using RunicMagic.World.Geometry;

namespace RunicMagic.Controller.Services;

internal class WorldRenderingService : IWorldRenderingService
{
    private readonly WorldModel world;
    private readonly RayCastService rayCast;

    public WorldRenderingService(WorldModel world, RayCastService rayCast)
    {
        this.world = world;
        this.rayCast = rayCast;
    }

    public IReadOnlyList<EntityRenderingModel> GetAllRenderingModels(Guid? casterEntityId)
    {
        var casterId = casterEntityId.HasValue ? new EntityId(casterEntityId.Value) : (EntityId?)null;

        var entities = world.GetAll();

        var indicateTargetId = casterId.HasValue
            ? world.Find(casterId.Value)?.IndicateTarget?.EntityId
            : null;

        var renderingModels = new List<EntityRenderingModel>();
        foreach (var entity in entities)
        {
            WorldCoordinate? pointingEnd = null;
            if (entity.PointingDirection.HasValue)
            {
                var castResult = rayCast.Cast(entity.Id, entity.Location, entity.PointingDirection.Value);
                pointingEnd = new WorldCoordinate(castResult.LocationOfIntersect.X, castResult.LocationOfIntersect.Y);
            }

            WorldCoordinate? indicateEnd = null;
            if (entity.IndicateTarget?.Direction != null)
            {
                var direction = entity.IndicateTarget.Direction.Value;
                var castResult = rayCast.Cast(entity.Id, entity.Location, direction, skipTranslucent: false);
                var dist = castResult.LocationOfIntersect.GetDistanceTo(entity.Location);

                var capped = Math.Min(dist, 1000);
                var endX = entity.Location.X + direction.X * capped;
                var endY = entity.Location.Y + direction.Y * capped;

                indicateEnd = new WorldCoordinate(endX, endY);
            }

            var isIndicateTarget = entity.Id == indicateTargetId;
            var mapped = EntityRenderingMapper.ToRenderingModel(entity, entity.Id == casterId, pointingEnd, isIndicateTarget, indicateEnd);
            renderingModels.Add(mapped);
        }

        return renderingModels;
    }
}
