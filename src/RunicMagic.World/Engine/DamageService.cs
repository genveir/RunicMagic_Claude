using RunicMagic.World.Entities;
using RunicMagic.World.Execution;

namespace RunicMagic.World.Engine;

public class DamageService
{
    private readonly WorldModel world;

    public DamageService(WorldModel world)
    {
        this.world = world;
    }

    public long Damage(EntitySet entitySet, long amount, IWorldEventTracker eventTracker)
    {
        var entities = entitySet.Entities.ToList();

        long remaining = amount;
        while (remaining > 0 && entities.Count > 0)
        {
            var perEntity = (long)Math.Ceiling((double)remaining / entities.Count);
            var nextEntities = new List<Entity>();
            foreach (var entity in entities)
            {
                var actual = Damage(entity, perEntity, eventTracker);
                remaining -= actual;
                if (entity.StructuralIntegrity.CurrentIntegrity > 0)
                {
                    nextEntities.Add(entity);
                }
            }
            entities = nextEntities;
        }

        return amount - remaining;
    }

    public long Damage(Entity entity, long amount, IWorldEventTracker eventTracker)
    {
        var actual = Math.Min(amount, entity.StructuralIntegrity.CurrentIntegrity);
        entity.StructuralIntegrity.CurrentIntegrity -= actual;

        if (entity.StructuralIntegrity.CurrentIntegrity == 0)
        {
            eventTracker.Add(new EntityDisintegratedEvent(entity));
            world.Remove(entity.Id);
        }

        if (entity.Life != null && entity.Life.CurrentHitPoints > entity.StructuralIntegrity.CurrentIntegrity)
        {
            entity.Life.CurrentHitPoints = entity.StructuralIntegrity.CurrentIntegrity;
        }

        if (entity.StructuralIntegrity.CurrentIntegrity > 0 && actual > 0)
        {
            eventTracker.Add(new EntityDamagedEvent(entity, actual));
        }
        return actual;
    }
}
