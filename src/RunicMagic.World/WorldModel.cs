using RunicMagic.World.Abstractions;
using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Motion;

namespace RunicMagic.World;

public class WorldModel : IGameLoopWorldModel
{
    private readonly Dictionary<EntityId, Entity> _entities = new();
    private readonly List<IMotionEffect> _motionEffects = new();

    public void Add(Entity entity)
    {
        _entities[entity.Id] = entity;
    }

    public void Remove(EntityId id)
    {
        var entity = _entities.GetValueOrDefault(id);

        if (entity != null)
        {
            _entities.Remove(id);
        }
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
        var sourceBounds = Bounds(source);
        var entities = _entities.Values
            .Where(e => e.Id != source.Id)
            .Where(e => Bounds(e).IsWithinDistanceFromRectangle(sourceBounds, distance))
            .ToList();
        return entities;
    }

    public IReadOnlyList<Entity> GetContainedEntities(Entity container)
    {
        var entities = _entities.Values.Where(e => e.Id != container.Id && Bounds(container).Contains(Bounds(e))).ToList();
        return entities;
    }

    public void AddMotionEffect(IMotionEffect effect)
    {
        _motionEffects.Add(effect);
    }

    public void TickMotion(IWorldEventTracker eventTracker)
    {
        List<IMotionEffect> effectsToRemove = new();
        foreach (var motionEffect in _motionEffects)
        {
            var advanced = motionEffect.TryAdvance(eventTracker);

            if (!advanced || motionEffect.IsComplete)
            {
                effectsToRemove.Add(motionEffect);
            }
        }
        _motionEffects.RemoveAll(effectsToRemove.Contains);
    }

    public void TickAI(IWorldEventTracker eventTracker)
    {
        foreach (var entity in _entities.Values)
        {
            if (entity.AI.BehaviorCount > 0)
            {
                entity.AI.Execute(entity, this, eventTracker);
            }
        }
    }

    private static Rectangle Bounds(Entity e)
    {
        var bounds = new Rectangle(e.Location, e.Width, e.Height, e.Angle);
        return bounds;
    }
}
