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
        services.AddSingleton(svc => new WorldModel(svc.GetRequiredService<EngineMotionCollection>()));
        services.AddSingleton<IGameLoopWorldModel>(svc => svc.GetRequiredService<WorldModel>());
        services.AddSingleton<IWorldTicker, WorldTicker>();
        services.AddSingleton<EngineMotionCollection>();

        services.AddSingleton<SpellExecutor>();
        services.AddSingleton<RayCastService>();

        services.AddSingleton<EngineAPI>();
        services.AddSingleton<EntitySetSelectService>();
        return services;
    }
}
