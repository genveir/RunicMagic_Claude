using RunicMagic.Controller.EntityConstruction;
using RunicMagic.Database;
using RunicMagic.World;
using RunicMagic.World.Entities;
using RunicMagic.World.Entities.AI;

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

        AddDebugHardcodedStuff();
    }

    private void AddDebugHardcodedStuff()
    {
        var guard = world.GetAll().FirstOrDefault(e => e.Label == "Guard");
        if (guard == null)
        {
            return;
        }

        guard.AI.AddBehavior(new WalkTowardPlayerBehavior());
    }

    private class WalkTowardPlayerBehavior : IAIBehavior
    {
        public void Execute(Entity entity, WorldModel worldModel, IWorldEventTracker eventTracker)
        {
            var player = worldModel.GetAll().FirstOrDefault(e => e.Label == "Player");
            if (player == null)
            {
                return;
            }

            entity.Locomotion!.Walk(entity, player.Location, eventTracker);
        }
    }
}
