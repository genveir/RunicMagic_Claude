namespace RunicMagic.World.Entities.AI;

public interface IAIBehavior
{
    void Execute(Entity entity, WorldModel worldModel, IWorldEventTracker eventTracker);
}
