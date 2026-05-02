using RunicMagic.World.Execution;

namespace RunicMagic.World.Abstractions;

public interface IGameLoopWorldModel
{
    void TickMotion(IWorldEventTracker eventTracker);

    void TickAI(IWorldEventTracker eventTracker);

    Entity? Find(EntityId id);
}
