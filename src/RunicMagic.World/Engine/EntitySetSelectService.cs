using RunicMagic.World.Entities;
using RunicMagic.World.Geometry;

namespace RunicMagic.World.Engine;

public class EntitySetSelectService
{
    // Maximum range from the origin at which we expect to actually simulate entities, beyond this we assume there is a living world with fictional entities.
    private const long FictionalBound = 100_000;

    private readonly WorldModel worldModel;
    private readonly RayCastService rayCastService;

    public EntitySetSelectService(WorldModel worldModel, RayCastService rayCastService)
    {
        this.worldModel = worldModel;
        this.rayCastService = rayCastService;
    }

    public EntitySetSelectionResult GetAll()
    {
        return new EntitySetSelectionResult(worldModel.GetAll(), 1_000_000_000);
    }

    public EntitySetSelectionResult GetAllInRangeFrom(IEnumerable<Entity> entities, long range)
    {
        var originEntities = entities.ToList();
        if (originEntities.Count == 0)
        {
            return new EntitySetSelectionResult(Array.Empty<Entity>(), 0);
        }

        var entitySet = new HashSet<Entity>();
        foreach (var entity in originEntities)
        {
            var nearbyEntities = worldModel.GetEntitiesWithinDistance(entity, range);
            foreach (var nearbyEntity in nearbyEntities)
            {
                entitySet.Add(nearbyEntity);
            }
        }

        var fictionalResults = GetFictionalResultForGetAllInRangeFrom(originEntities.Select(e => e.Location).Centroid(), range);

        return new EntitySetSelectionResult(entitySet.ToList(), fictionalResults);
    }

    // slightly undervalues the number of entities, centroid will always be more central to the in-bounds area than any of the entities.
    private long GetFictionalResultForGetAllInRangeFrom(Location centroid, long range)
    {
        // assume 1 entity per meter squared, so calculate surface of the circle from the centroid, then subtract the area within the fictional bound.
        var r = (double)range;
        var R = (double)FictionalBound;
        var origin = new Location(0, 0);
        var d = centroid.GetDistanceTo(origin);

        double intersectionArea;
        if (d >= r + R)
        {
            intersectionArea = 0;
        }
        else if (d + r <= R)
        {
            // query circle entirely inside fictional bound
            intersectionArea = Math.PI * r * r;
        }
        else if (d + R <= r)
        {
            // fictional bound entirely inside query circle
            intersectionArea = Math.PI * R * R;
        }
        else
        {
            var alpha = Math.Acos((d * d + r * r - R * R) / (2 * d * r));
            var beta = Math.Acos((d * d + R * R - r * r) / (2 * d * R));
            intersectionArea = r * r * alpha + R * R * beta - 0.5 * Math.Sqrt((-d + r + R) * (d + r - R) * (d - r + R) * (d + r + R));
        }

        var queryArea = Math.PI * r * r;
        var fictionalArea = Math.Max(0, queryArea - intersectionArea);
        var result = (long)(fictionalArea / 1_000_000);
        return result;
    }

    public EntitySetSelectionResult GetAllInRay(Location from, Direction direction, long range)
    {
        var entitiesOnRay = rayCastService.GetAllEntitiesOnRay(from, direction, range);

        var fictionalResults = GetFictionalResultForGetAllInRay(from, direction, range);

        return new EntitySetSelectionResult(entitiesOnRay, fictionalResults);
    }

    public EntitySetSelectionResult GetFirstInRay(Location from, Direction direction, long range)
    {
        var selection = GetAllInRay(from, direction, range);

        var entitiesOnRay = selection.Entities;
        var fictionalResults = selection.FictionalResults;

        var firstEntity = entitiesOnRay.FirstOrDefault();

        if (firstEntity != null)
        {
            return new EntitySetSelectionResult([firstEntity], 0);
        }

        return new EntitySetSelectionResult(Array.Empty<Entity>(), fictionalResults > 0 ? 1 : 0);
    }

    private long GetFictionalResultForGetAllInRay(Location from, Direction direction, long range)
    {
        // assume 1 entity per 10 meters outside of the fictional bound
        // ray: P(t) = from + t * direction, t in [0, range]; solve for intersection with circle radius FictionalBound
        var R = (double)FictionalBound;
        var b = 2 * (from.X * direction.X + from.Y * direction.Y);
        var c = from.X * from.X + from.Y * from.Y - R * R;
        var discriminant = b * b - 4 * c;

        double fictionalLength;
        if (discriminant < 0)
        {
            // ray misses the fictional bound circle entirely; from is outside
            fictionalLength = range;
        }
        else
        {
            var sqrtDisc = Math.Sqrt(discriminant);
            var t1 = (-b - sqrtDisc) / 2;
            var t2 = (-b + sqrtDisc) / 2;
            var insideStart = Math.Max(0, t1);
            var insideEnd = Math.Min((double)range, t2);
            var insideLength = Math.Max(0, insideEnd - insideStart);
            fictionalLength = range - insideLength;
        }

        var result = (long)(fictionalLength / 10_000);
        return result;
    }

    public EntitySetSelectionResult GetUnionScope(IEnumerable<Entity> entities)
    {
        IEnumerable<Entity> union = new List<Entity>();
        long fictionalResults = 0;

        foreach (var entity in entities)
        {
            var scope = entity.Scope?.Invoke() ?? new EntitySetSelectionResult([], 0);

            fictionalResults = Math.Max(fictionalResults, scope.FictionalResults);

            union = union.Union(scope.Entities);
        }

        return new EntitySetSelectionResult(union.ToList(), fictionalResults);
    }

    public EntitySetSelectionResult GetIntersectScope(IEnumerable<Entity> entities)
    {
        if (entities.Count() == 0)
        {
            return new EntitySetSelectionResult(Array.Empty<Entity>(), 0);
        }

        long fictionalResults = long.MaxValue;

        IEnumerable<Entity> intersect = new List<Entity>();

        bool isFirst = true;

        foreach (var entity in entities)
        {
            var scope = entity.Scope?.Invoke() ?? new EntitySetSelectionResult([], 0);

            fictionalResults = Math.Min(fictionalResults, scope.FictionalResults);

            if (isFirst)
            {
                intersect = scope.Entities;
                isFirst = false;
            }
            else
            {
                intersect = intersect.Intersect(scope.Entities);
            }
        }

        return new EntitySetSelectionResult(intersect.ToList(), fictionalResults);
    }
}
