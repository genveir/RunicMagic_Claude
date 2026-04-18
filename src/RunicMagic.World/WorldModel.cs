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

    public IReadOnlyList<Entity> GetEntitiesAtPoint(int x, int y)
    {
        var entities = _entities.Values.Where(e => Bounds(e).Contains(x, y)).ToList();
        return entities;
    }

    public IReadOnlyList<Entity> GetEntitiesInArea(Rectangle area)
    {
        var entities = _entities.Values.Where(e => Bounds(e).IntersectsWith(area)).ToList();
        return entities;
    }

    public IReadOnlyList<Entity> GetTouchingEntities(Entity source)
    {
        var entities = _entities.Values.Where(e => e.Id != source.Id && Bounds(source).Touches(Bounds(e))).ToList();
        return entities;
    }

    public IReadOnlyList<Entity> GetContainedEntities(Entity container)
    {
        var entities = _entities.Values.Where(e => e.Id != container.Id && Bounds(container).Contains(Bounds(e))).ToList();
        return entities;
    }

    private static Rectangle Bounds(Entity e)
    {
        var bounds = new Rectangle(e.X, e.Y, e.Width, e.Height);
        return bounds;
    }
}
