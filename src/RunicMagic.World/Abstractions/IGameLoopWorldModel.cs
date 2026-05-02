using RunicMagic.World.Execution;

namespace RunicMagic.World.Abstractions;

public interface IGameLoopWorldModel
{
    void HandleTick(IWorldEventTracker eventTracker);

    Entity? Find(EntityId id);
}
