using RunicMagic.Controller.EntityConstruction;
using RunicMagic.Database;
using RunicMagic.World;
using RunicMagic.World.AI;
using RunicMagic.World.Capabilities;
using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Services;

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
            var direction = new Direction(Math.Cos(entity.Angle), Math.Sin(entity.Angle));
            LocomotionService.TryMove(entity, direction, eventTracker);
        }
    }
}
