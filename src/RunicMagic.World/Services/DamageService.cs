namespace RunicMagic.World.Services;

public class DamageService
{
    public long Damage(Entity entity, long amount, WorldModel world)
    {
        var actual = Math.Min(amount, entity.StructuralIntegrity.CurrentIntegrity);
        entity.StructuralIntegrity.CurrentIntegrity -= actual;

        if (entity.StructuralIntegrity.CurrentIntegrity == 0)
        {
            world.Remove(entity.Id);
        }

        if (entity.Life != null && entity.Life.CurrentHitPoints > entity.StructuralIntegrity.CurrentIntegrity)
        {
            entity.Life.CurrentHitPoints = entity.StructuralIntegrity.CurrentIntegrity;
        }

        return actual;
    }
}
