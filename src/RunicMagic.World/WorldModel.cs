using RunicMagic.World.Abstractions;
using RunicMagic.World.Entities;
using RunicMagic.World.Geometry;
using RunicMagic.World.Motion.Engine;

namespace RunicMagic.World;

public class WorldModel : IGameLoopWorldModel
{
    private readonly Dictionary<EntityId, Entity> _entities = new();
    private readonly List<IEngineMotionEffect> _engineMotionEffects = new();

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

    public void AddMotionEffect(IEngineMotionEffect effect)
    {
        _engineMotionEffects.Add(effect);
    }

    public void HandleTick(IWorldEventTracker eventTracker)
    {
        var entitiesUnderMotion = TickEngineMotion(eventTracker);

        TickEntities(eventTracker, entitiesUnderMotion);
    }

    private HashSet<Entity> TickEngineMotion(IWorldEventTracker eventTracker)
    {
        HashSet<Entity> entitiesUnderEngineMotion = [];

        List<IEngineMotionEffect> effectsToRemove = new();
        foreach (var engineMotionEffect in _engineMotionEffects)
        {
            var (advanced, entities) = engineMotionEffect.TryAdvance(eventTracker);

            if (!advanced || engineMotionEffect.IsComplete)
            {
                effectsToRemove.Add(engineMotionEffect);
            }

            foreach (var entity in entities)
            {
                entitiesUnderEngineMotion.Add(entity);
            }
        }
        _engineMotionEffects.RemoveAll(effectsToRemove.Contains);

        return entitiesUnderEngineMotion;
    }

    private void TickEntities(IWorldEventTracker eventTracker, HashSet<Entity> entitiesUnderEngineMotion)
    {
        foreach (var entity in _entities.Values)
        {
            entity.IsUnderEngineMotion = entitiesUnderEngineMotion.Contains(entity);

            TickAI(eventTracker, entity);
        }
    }

    private void TickAI(IWorldEventTracker eventTracker, Entity entity)
    {
        if (entity.AI.BehaviorCount > 0)
        {
            entity.AI.Execute(entity, this, eventTracker);
        }
    }

    private static Rectangle Bounds(Entity e)
    {
        var bounds = new Rectangle(e.Location, e.Width, e.Height, e.Angle);
        return bounds;
    }
}
