using RunicMagic.World.Execution;

namespace RunicMagic.World.Services;

internal static class DamageService
{
    public static long Damage(EntitySet entitySet, long amount, SpellContext context)
    {
        var entities = entitySet.Entities.ToList();

        long remaining = amount;
        while (remaining > 0 && entities.Count > 0)
        {
            var perEntity = (long)Math.Ceiling((double)remaining / entities.Count);
            var nextEntities = new List<Entity>();
            foreach (var entity in entities)
            {
                var actual = Damage(entity, perEntity, context);
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

    public static long Damage(Entity entity, long amount, SpellContext context)
    {
        var actual = Math.Min(amount, entity.StructuralIntegrity.CurrentIntegrity);
        entity.StructuralIntegrity.CurrentIntegrity -= actual;

        if (entity.StructuralIntegrity.CurrentIntegrity == 0)
        {
            context.EventTracker.Add(new EntityDisintegratedEvent(entity));
            context.World.Remove(entity.Id);
        }

        if (entity.Life != null && entity.Life.CurrentHitPoints > entity.StructuralIntegrity.CurrentIntegrity)
        {
            entity.Life.CurrentHitPoints = entity.StructuralIntegrity.CurrentIntegrity;
        }

        if (entity.StructuralIntegrity.CurrentIntegrity > 0 && actual > 0)
        {
            context.EventTracker.Add(new EntityDamagedEvent(entity, actual));
        }
        return actual;
    }
}
