using Microsoft.Extensions.DependencyInjection;
using RunicMagic.World.Abstractions;
using RunicMagic.World.Engine;
using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Motion.Engine;

namespace RunicMagic.World;

public static class WorldModule
{
    public static IServiceCollection RegisterWorldModule(this IServiceCollection services)
    {
        services.AddSingleton(svc => new WorldModel());
        services.AddSingleton<IGameLoopWorldModel>(svc => svc.GetRequiredService<WorldModel>());
        services.AddSingleton<IWorldTicker, WorldTicker>();
        services.AddSingleton<EngineMotionCollection>();
        services.AddSingleton<IEngineMotionSink>(svc => svc.GetRequiredService<EngineMotionCollection>());

        services.AddSingleton<SpellExecutor>();
        services.AddSingleton<RayCastService>();

        services.AddSingleton<EngineAPI>();
        services.AddSingleton<EntitySetSelectService>();
        services.AddSingleton<DamageService>();
        services.AddSingleton<PowerService>();
        return services;
    }
}
