namespace RunicMagic.World.Execution;

public interface IWorldEventTracker
{
    public void Add(WorldEvent @event);
    public void Track(Entity entity);
}
