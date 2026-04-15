using Microsoft.Extensions.DependencyInjection;
using RunicMagic.Database;
using RunicMagic.World;

namespace RunicMagic.Controller;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection RegisterApplicationModules(this IServiceCollection services, string connectionString)
    {
        services.RegisterControllerModule();
        services.RegisterWorldModule();
        services.RegisterDatabaseModule(connectionString);

        return services;
    }
}
