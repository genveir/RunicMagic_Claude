using RunicMagic.Controller.EntityConstruction;
using RunicMagic.Database;
using RunicMagic.World;
using RunicMagic.World.Entities;
using RunicMagic.World.Entities.AI;
using RunicMagic.World.Entities.Capabilities;
using RunicMagic.World.Motion.Simulated;

namespace RunicMagic.Controller.Services;

public class WorldLoadingService(WorldLoader loader, EntityFactory factory, WorldModel world)
{
    public async Task LoadAsync()
    {
        var entities = await loader.LoadAsync();

        foreach (var entityData in entities)
        {
            world.Add(factory.Create(entityData));
        }

        AddDebugHardcodedStuff(world);
    }

    private void AddDebugHardcodedStuff(WorldModel world)
    {
        var guard = world.GetAll().FirstOrDefault(e => e.Label == "Guard");

        guard!.Locomotion = new LocomotionCapability();
        guard.AI.AddBehavior(new RunAheadBehavior());
    }

    private class RunAheadBehavior : IAIBehavior
    {
        public void Execute(Entity entity, WorldModel worldModel, IWorldEventTracker eventTracker)
        {
            var fx = 100_000 * Math.Cos(entity.Angle);
            var fy = 100_000 * Math.Sin(entity.Angle);
            entity.PendingImpulses.Add(new ForceVector(fx, fy));
        }
    }
}
