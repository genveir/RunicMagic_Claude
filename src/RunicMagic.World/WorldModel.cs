using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Motion;

namespace RunicMagic.World;

public class WorldModel
{
    private readonly Dictionary<EntityId, Entity> _entities = new();
    private readonly List<IMotionEffect> _motionEffects = new();

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

    public SpellResult TickMotion()
    {
        var result = new SpellResult();

        List<IMotionEffect> effectsToRemove = new();
        foreach (var motionEffect in _motionEffects)
        {
            var advanced = motionEffect.TryAdvance(result);

            if (!advanced || motionEffect.IsComplete)
            {
                effectsToRemove.Add(motionEffect);
            }
        }
        _motionEffects.RemoveAll(effectsToRemove.Contains);

        return result;
    }

    private static Rectangle Bounds(Entity e)
    {
        var bounds = new Rectangle(e.Location, e.Width, e.Height, e.Angle);
        return bounds;
    }
}
