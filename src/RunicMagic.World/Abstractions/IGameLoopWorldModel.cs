using RunicMagic.World.Entities;

namespace RunicMagic.World.Abstractions;

public interface IGameLoopWorldModel
{
    void HandleTick(IWorldEventTracker eventTracker, long currentTick);

    Entity? Find(EntityId id);
}
