using RunicMagic.World.Motion.Engine;

namespace RunicMagic.World.Engine;

public class EngineAPI
{
    public EntitySetSelectService EntitySetSelectService { get; }
    public DamageService DamageService { get; }
    public PowerService PowerService { get; }
    public IEngineMotionSink EngineMotion { get; }

    public EngineAPI(
        EntitySetSelectService entitySetSelectService,
        DamageService damageService,
        PowerService powerService,
        IEngineMotionSink engineMotion)
    {
        EntitySetSelectService = entitySetSelectService;
        DamageService = damageService;
        PowerService = powerService;
        EngineMotion = engineMotion;
    }
}
