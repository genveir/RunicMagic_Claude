using RunicMagic.World;
using RunicMagic.World.Motion.Engine;

namespace RunicMagic.Tests.Builders;

internal class WorldTickerBuilder
{
    private WorldModel worldModel;
    private EngineMotionCollection engineMotionCollection;

    public WorldTickerBuilder()
    {
        engineMotionCollection = new EngineMotionCollection();
        worldModel = new WorldModelBuilder()
            .WithEngineMotionCollection(engineMotionCollection)
            .Build();
    }

    public static WorldTickerBuilder ForWorldModel(WorldModel worldModel)
    {
        return new WorldTickerBuilder()
            .WithWorldModel(worldModel, true);
    }

    public WorldTickerBuilder WithWorldModel(WorldModel worldModel, bool captureEngineMotionCollection = true)
    {
        this.worldModel = worldModel;

        if (captureEngineMotionCollection)
        {
            engineMotionCollection = GetPrivateFieldsForTesting.GetEngineMotionCollection(worldModel);
        }

        return this;
    }

    public WorldTickerBuilder WithEngineMotionCollection(EngineMotionCollection engineMotionCollection)
    {
        this.engineMotionCollection = engineMotionCollection;
        return this;
    }

    public WorldTicker Build()
    {
        return new WorldTicker(worldModel, engineMotionCollection);
    }
}
