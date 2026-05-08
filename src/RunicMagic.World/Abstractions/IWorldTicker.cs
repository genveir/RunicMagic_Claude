namespace RunicMagic.World.Abstractions;

public interface IWorldTicker
{
    void HandleTick(IWorldEventTracker eventTracker, long currentTick);
}
