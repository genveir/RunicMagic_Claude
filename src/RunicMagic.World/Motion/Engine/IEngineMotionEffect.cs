using RunicMagic.World.Entities;

namespace RunicMagic.World.Motion.Engine;

public readonly record struct EngineMotionEffectResult(bool Advanced, IEnumerable<Entity> EntitiesUnderMotion);

public interface IEngineMotionEffect
{
    EngineMotionEffectResult TryAdvance(IWorldEventTracker tickResult);
    bool IsComplete { get; }
}
