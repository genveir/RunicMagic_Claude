namespace RunicMagic.World;

public class WorldModel
{
    private readonly Dictionary<EntityId, Entity> _entities = new();

    public void Add(Entity entity)    => _entities[entity.Id] = entity;
    public void Remove(EntityId id)   => _entities.Remove(id);
    public Entity? Find(EntityId id)  => _entities.GetValueOrDefault(id);

    public IReadOnlyList<Entity> GetEntitiesInArea(Rectangle area) =>
        [.. _entities.Values.Where(e => e.Bounds.IntersectsWith(area))];

    public IReadOnlyList<Entity> GetTouchingEntities(Entity source) =>
        [.. _entities.Values.Where(e => e.Id != source.Id && source.Bounds.Touches(e.Bounds))];

    public IReadOnlyList<Entity> GetContainedEntities(Entity container) =>
        [.. _entities.Values.Where(e => e.Id != container.Id && container.Bounds.Contains(e.Bounds))];
}
