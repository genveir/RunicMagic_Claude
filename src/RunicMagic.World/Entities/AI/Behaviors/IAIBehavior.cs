namespace RunicMagic.World.Entities.AI.Behaviors;

public interface IAIBehavior
{
    void Execute(Entity entity, WorldModel worldModel, IWorldEventTracker eventTracker, long currentTick);
}
