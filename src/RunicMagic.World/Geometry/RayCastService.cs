using RunicMagic.World.Entities;

namespace RunicMagic.World.Geometry;

public class RayCastService
{
    private const long MaxRangeMillimetres = 3000;

    private readonly WorldModel world;

    public RayCastService(WorldModel world)
    {
        this.world = world;
    }

    public RayCastResult Cast(EntityId sourceId, Location origin, Direction direction, bool skipTranslucent = true)
    {
        var closestT = double.MaxValue;
        Entity? closestEntity = null;

        foreach (var entity in world.GetAll())
        {
            if (entity.Id == sourceId) continue;
            if (skipTranslucent && entity.IsTranslucent) continue;

            var bounds = entity.Bounds;
            if (bounds.IntersectsRay(origin, direction, out var t))
            {
                if (t < closestT)
                {
                    closestT = t;
                    closestEntity = entity;
                }
            }
        }

        var range = closestT == double.MaxValue ? MaxRangeMillimetres : closestT;
        var end = new Location
        {
            X = origin.X + direction.X * range,
            Y = origin.Y + direction.Y * range
        };
        return new RayCastResult(closestEntity, end);
    }
}
