using RunicMagic.World.Entities;

namespace RunicMagic.World.Abstractions;

public interface IGameLoopWorldModel
{
    Entity? Find(EntityId id);
}
