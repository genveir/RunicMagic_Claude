using RunicMagic.World.Execution;

namespace RunicMagic.World.Motion;

public interface IMotionEffect
{
    bool TryAdvance(IWorldEventTracker tickResult);
    bool IsComplete { get; }
}
