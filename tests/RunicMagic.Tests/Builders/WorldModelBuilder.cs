using RunicMagic.World;
using RunicMagic.World.Motion.Engine;

namespace RunicMagic.Tests.Builders;

internal class WorldModelBuilder
{
    private EngineMotionCollection engineMotionCollection = new EngineMotionCollection();

    public WorldModelBuilder() { }

    public WorldModelBuilder WithEngineMotionCollection(EngineMotionCollection engineMotionCollection)
    {
        this.engineMotionCollection = engineMotionCollection;
        return this;
    }

    public WorldModel Build()
    {
        var worldModel = new WorldModel(engineMotionCollection);
        return worldModel;
    }
}
