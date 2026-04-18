using RunicMagic.World.Geometry;

namespace RunicMagic.World;

public class RayCastService(WorldModel world)
{
    private const int MaxRangeMillimetres = 3000;

    public RayCastResult Cast(EntityId sourceId, int originX, int originY, Direction direction)
    {
        var closestT = double.MaxValue;
        Entity? closestEntity = null;

        foreach (var entity in world.GetAll())
        {
            if (entity.Id == sourceId) continue;
            if (entity.IsTranslucent) continue;

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
        var endX = (int)Math.Round(originX + direction.X * range);
        var endY = (int)Math.Round(originY + direction.Y * range);
        return new RayCastResult(closestEntity, endX, endY);
    }
}
