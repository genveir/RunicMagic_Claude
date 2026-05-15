using RunicMagic.Controller.Services;
using RunicMagic.Tests.Builders;
using RunicMagic.World;
using RunicMagic.World.Engine;
using RunicMagic.World.Execution;
using RunicMagic.World.Motion.Engine;

namespace RunicMagic.Tests.TestUtilities;

// Bundles a WorldModel, the EngineMotionCollection, and an EngineAPI wired against both,
// so spell-driven motion effects land in the same collection the WorldTicker processes.
internal sealed class SpellHarness
{
    public WorldModel World { get; }
    public EngineMotionCollection EngineMotion { get; }
    public EngineAPI EngineAPI { get; }

    public SpellHarness()
    {
        World = new WorldModel();
        EngineMotion = new EngineMotionCollection();
        EngineAPI = EngineAPIBuilder.ForWorldModel(World).WithEngineMotionCollection(EngineMotion).Build();
    }

    public SpellContext MakeContext(
        EntitySet? caster = null,
        EntitySet? executor = null,
        EventTracker? result = null)
    {
        var context = TestFixtures.MakeContext(caster: caster, executor: executor, engineAPI: EngineAPI, result: result);
        return context;
    }

    public WorldTicker BuildTicker()
    {
        var ticker = WorldTickerBuilder.ForWorldModel(World).WithEngineMotionCollection(EngineMotion).Build();
        return ticker;
    }
}
