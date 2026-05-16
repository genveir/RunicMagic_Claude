using RunicMagic.World.Entities;

namespace RunicMagic.World.Geometry;

public class RayCastService
{
    private readonly record struct IntermediateResult(Entity Entity, double Distance);

    private readonly WorldModel world;

    public RayCastService(WorldModel world)
    {
        this.world = world;
    }

    public RayCastResult Cast(EntityId sourceId, Location origin, Direction direction, int maxRangeMillimeters, bool skipTranslucent = true)
    {
        var entitiesOnRay = GetAllEntitiesOnRayWithDistance(origin, direction, int.MaxValue);

        var filtered = entitiesOnRay
            .Where(r => r.Entity.Id != sourceId)
            .Where(r => !skipTranslucent || !r.Entity.IsTranslucent);

        IntermediateResult? closestEntity = null;
        if (filtered.Any())
        {
            closestEntity = filtered.OrderBy(r => r.Distance).First();
        }

        var range = closestEntity.HasValue ? closestEntity.Value.Distance : maxRangeMillimeters;
        var end = new Location
        {
            X = origin.X + direction.X * range,
            Y = origin.Y + direction.Y * range
        };
        return new RayCastResult(closestEntity?.Entity, end);
    }

    public IReadOnlyList<Entity> GetAllEntitiesOnRay(Location origin, Direction direction, long range)
    {
        var entitiesOnRay = GetAllEntitiesOnRayWithDistance(origin, direction, range);

        return entitiesOnRay
            .OrderBy(r => r.Distance)
            .Select(r => r.Entity)
            .ToList();
    }

    private IEnumerable<IntermediateResult> GetAllEntitiesOnRayWithDistance(Location origin, Direction direction, long range)
    {
        foreach (var entity in world.GetAll())
        {
            var bounds = entity.Bounds;
            if (bounds.IntersectsRay(origin, direction, out var distance))
            {
                if (distance <= range)
                {
                    yield return new IntermediateResult(entity, distance);
                }
            }
        }
    }
}
