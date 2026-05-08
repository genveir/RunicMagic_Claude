using RunicMagic.World;
using RunicMagic.World.Motion.Engine;

namespace RunicMagic.Tests.TestUtilities;

internal static class GetPrivateFieldsForTesting
{
    internal static EngineMotionCollection GetEngineMotionCollection(WorldModel world)
    {
        var engineMotionCollectionField = typeof(WorldModel)
            .GetField("engineMotionCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var engineMotionCollection = engineMotionCollectionField?.GetValue(world) as EngineMotionCollection;

        return engineMotionCollection ?? throw new InvalidOperationException("Could not extract EngineMotionCollection from WorldModel.");
    }
}
