using RunicMagic.World.Abstractions;
using RunicMagic.World.Entities;
using RunicMagic.World.Geometry;
using RunicMagic.World.Motion.Engine;

namespace RunicMagic.World;

public class WorldModel : IGameLoopWorldModel
{
    private readonly Dictionary<EntityId, Entity> entities = new();
    private readonly EngineMotionCollection engineMotionCollection;

    internal WorldModel(EngineMotionCollection engineMotionCollection)
    {
        this.engineMotionCollection = engineMotionCollection;
    }

    public void Add(Entity entity)
    {
        entities[entity.Id] = entity;
    }

    public void Remove(EntityId id)
    {
        var entity = entities.GetValueOrDefault(id);

        if (entity != null)
        {
            entities.Remove(id);
        }
    }

    public Entity? Find(EntityId id)
    {
        var entity = entities.GetValueOrDefault(id);
        return entity;
    }

    public IReadOnlyList<Entity> GetAll()
    {
        return entities.Values.ToList();
    }

    public IReadOnlyList<Entity> GetEntitiesAtPoint(Location location)
    {
        return entities.Values.Where(e => e.Bounds.Contains(location)).ToList();
    }

    public IReadOnlyList<Entity> GetEntitiesWithinDistance(Entity source, double distance)
    {
        var sourceBounds = source.Bounds;
        return entities.Values
            .Where(e => e.Id != source.Id)
            .Where(e => e.Bounds.IsWithinDistanceFromRectangle(sourceBounds, distance))
            .ToList();
    }

    public IReadOnlyList<Entity> GetContainedEntities(Entity container)
    {
        return entities.Values.Where(e => e.Id != container.Id && container.Bounds.Contains(e.Bounds)).ToList();
    }

    public void AddMotionEffect(IEngineMotionEffect effect)
    {
        engineMotionCollection.AddMotionEffect(effect);
    }
}
