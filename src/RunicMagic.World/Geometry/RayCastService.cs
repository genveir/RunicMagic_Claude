namespace RunicMagic.World.Geometry;

public class RayCastService(WorldModel world)
{
    private const long MaxRangeMillimetres = 3000;

    public RayCastResult Cast(EntityId sourceId, Location origin, Direction direction, bool skipTranslucent = true)
    {
        var closestT = double.MaxValue;
        Entity? closestEntity = null;

        foreach (var entity in world.GetAll())
        {
            if (entity.Id == sourceId) continue;
            if (skipTranslucent && entity.IsTranslucent) continue;

            var bounds = new Rectangle(entity.Location, entity.Width, entity.Height);
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
