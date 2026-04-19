using RunicMagic.World.Geometry;

namespace RunicMagic.World;

public class WorldModel
{
    private readonly Dictionary<EntityId, Entity> _entities = new();

    public void Add(Entity entity)
    {
        _entities[entity.Id] = entity;
    }

    public void Remove(EntityId id)
    {
        _entities.Remove(id);
    }

    public Entity? Find(EntityId id)
    {
        var entity = _entities.GetValueOrDefault(id);
        return entity;
    }

    public IReadOnlyList<Entity> GetAll()
    {
        var entities = _entities.Values.ToList();
        return entities;
    }

    public IReadOnlyList<Entity> GetEntitiesAtPoint(Location location)
    {
        var entities = _entities.Values.Where(e => Bounds(e).Contains(location)).ToList();
        return entities;
    }

    public IReadOnlyList<Entity> GetEntitiesWithinDistance(Entity source, double distance)
    {
        var entities = _entities.Values
            .Where(e => e.Id != source.Id)
            .WithinDistance(source.Location, distance)
            .ToList();
        return entities;
    }

    public IReadOnlyList<Entity> GetContainedEntities(Entity container)
    {
        var entities = _entities.Values.Where(e => e.Id != container.Id && Bounds(container).Contains(Bounds(e))).ToList();
        return entities;
    }

    private static Rectangle Bounds(Entity e)
    {
        var bounds = new Rectangle(e.Location, e.Width, e.Height, e.Angle);
        return bounds;
    }
}
