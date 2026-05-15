using RunicMagic.World;
using RunicMagic.World.Engine;
using RunicMagic.World.Geometry;
using RunicMagic.World.Motion.Engine;

namespace RunicMagic.Tests.Builders;

internal class EngineAPIBuilder
{
    private WorldModel worldModel = new WorldModel();
    private EngineMotionCollection engineMotionCollection = new EngineMotionCollection();

    public static EngineAPIBuilder ForWorldModel(WorldModel worldModel)
    {
        return new EngineAPIBuilder().WithWorldModel(worldModel);
    }

    public EngineAPIBuilder WithWorldModel(WorldModel worldModel)
    {
        this.worldModel = worldModel;
        return this;
    }

    public EngineAPIBuilder WithEngineMotionCollection(EngineMotionCollection engineMotionCollection)
    {
        this.engineMotionCollection = engineMotionCollection;
        return this;
    }

    public EngineAPI Build()
    {
        var rayCastService = new RayCastService(worldModel);
        var entitySetSelectService = new EntitySetSelectService(worldModel, rayCastService);
        var damageService = new DamageService(worldModel);
        var powerService = new PowerService(damageService, entitySetSelectService);
        return new EngineAPI(entitySetSelectService, damageService, powerService, engineMotionCollection);
    }
}
