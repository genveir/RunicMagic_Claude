using Microsoft.Extensions.DependencyInjection;
using RunicMagic.World.Geometry;

namespace RunicMagic.World;

public static class WorldModule
{
    public static IServiceCollection RegisterWorldModule(this IServiceCollection services)
    {
        services.AddSingleton<WorldModel>();
        services.AddSingleton<SpellExecutor>();
        services.AddSingleton<TeleportEntityService>();
        services.AddSingleton<RayCastService>();
        return services;
    }
}
