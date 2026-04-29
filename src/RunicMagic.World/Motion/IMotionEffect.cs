using RunicMagic.World.Execution;

namespace RunicMagic.World.Motion;

public interface IMotionEffect
{
    bool TryAdvance(SpellResult tickResult);
    bool IsComplete { get; }
}
