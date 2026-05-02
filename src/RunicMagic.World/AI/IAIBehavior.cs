using RunicMagic.World.Execution;

namespace RunicMagic.World.AI;

public interface IAIBehavior
{
    void Execute(Entity entity, WorldModel worldModel, IWorldEventTracker eventTracker);
}
