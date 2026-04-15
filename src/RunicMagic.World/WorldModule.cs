using Microsoft.Extensions.DependencyInjection;

namespace RunicMagic.World;

public static class WorldModule
{
    public static IServiceCollection RegisterWorldModule(this IServiceCollection services)
    {
        services.AddSingleton<WorldModel>();
        return services;
    }
}
