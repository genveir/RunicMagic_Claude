using RunicMagic.World;
using RunicMagic.World.Motion.Engine;

namespace RunicMagic.Tests.TestUtilities;

internal static class GetPrivateFieldsForTesting
{
    internal static List<IEngineMotionEffect> GetMotionEffects(WorldModel world)
    {
        var effects = typeof(WorldModel)
            .GetField("_engineMotionEffects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .GetValue(world) as List<IEngineMotionEffect>;

        return effects!;
    }
}
