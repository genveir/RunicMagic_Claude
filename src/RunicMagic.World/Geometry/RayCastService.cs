namespace RunicMagic.World.Geometry;

public class RayCastService(WorldModel world)
{
    private const long MaxRangeMillimetres = 3000;

    public RayCastResult Cast(EntityId sourceId, long originX, long originY, Direction direction, bool skipTranslucent = true)
    {
        var closestT = double.MaxValue;
        Entity? closestEntity = null;

        foreach (var entity in world.GetAll())
        {
            if (entity.Id == sourceId) continue;
            if (skipTranslucent && entity.IsTranslucent) continue;

            var bounds = new Rectangle(entity.X, entity.Y, entity.Width, entity.Height);
            if (bounds.IntersectsRay(originX, originY, direction.X, direction.Y, out var t))
            {
                if (t < closestT)
                {
                    closestT = t;
                    closestEntity = entity;
                }
            }
        }

        var range = closestT == double.MaxValue ? MaxRangeMillimetres : closestT;
        var endX = (long)Math.Round(originX + direction.X * range);
        var endY = (long)Math.Round(originY + direction.Y * range);
        return new RayCastResult(closestEntity, endX, endY);
    }
}
