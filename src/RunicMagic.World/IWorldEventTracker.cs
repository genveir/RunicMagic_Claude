using RunicMagic.World.Entities;
using RunicMagic.World.Execution;

namespace RunicMagic.World;

public interface IWorldEventTracker
{
    public void Add(WorldEvent @event);
    public void Track(Entity entity);
}
