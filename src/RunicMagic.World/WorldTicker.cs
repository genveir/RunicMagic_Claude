using RunicMagic.World.Abstractions;
using RunicMagic.World.Entities;
using RunicMagic.World.Motion.Engine;
using RunicMagic.World.Motion.Simulated;

namespace RunicMagic.World;

internal class WorldTicker : IWorldTicker
{
    private readonly WorldModel world;
    private readonly EngineMotionCollection engineMotionCollection;

    public WorldTicker(WorldModel world, EngineMotionCollection engineMotionCollection)
    {
        this.world = world;
        this.engineMotionCollection = engineMotionCollection;
    }

    public void HandleTick(IWorldEventTracker eventTracker, long currentTick)
    {
        var entities = world.GetAll();
        var entitiesUnderMotion = TickEngineMotion(eventTracker);

        TickEntities(entities, eventTracker, entitiesUnderMotion, currentTick);
        TickPhysics(entities, eventTracker);
    }

    private HashSet<Entity> TickEngineMotion(IWorldEventTracker eventTracker)
    {
        HashSet<Entity> entitiesUnderEngineMotion = [];

        List<IEngineMotionEffect> effectsToRemove = new();
        foreach (var engineMotionEffect in engineMotionCollection.GetAll())
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
        engineMotionCollection.RemoveAll(effectsToRemove.Contains);

        return entitiesUnderEngineMotion;
    }

    private void TickEntities(IEnumerable<Entity> entities, IWorldEventTracker eventTracker, HashSet<Entity> entitiesUnderEngineMotion, long currentTick)
    {
        foreach (var entity in entities)
        {
            entity.IsUnderEngineMotion = entitiesUnderEngineMotion.Contains(entity);

            TickAI(eventTracker, entity, currentTick);
        }
    }

    private void TickAI(IWorldEventTracker eventTracker, Entity entity, long currentTick)
    {
        if (entity.AI.BehaviorCount > 0)
        {
            entity.AI.Execute(entity, world, eventTracker, currentTick);
        }
    }

    private void TickPhysics(IEnumerable<Entity> entities, IWorldEventTracker eventTracker)
    {
        PhysicsService.Tick(entities, eventTracker);
    }
}
