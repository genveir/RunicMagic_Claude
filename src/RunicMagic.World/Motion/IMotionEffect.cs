using RunicMagic.World.Execution;

namespace RunicMagic.World.Motion;

public readonly record struct MotionEffectResult(bool Advanced, IEnumerable<Entity> EntitiesUnderMotion);

public interface IMotionEffect
{
    MotionEffectResult TryAdvance(IWorldEventTracker tickResult);
    bool IsComplete { get; }
}
