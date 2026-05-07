using RunicMagic.Database;
using RunicMagic.View;
using RunicMagic.World;

namespace RunicMagic.Controller;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection RegisterApplicationModules(this IServiceCollection services, string connectionString)
    {
        services.RegisterViewModule();
        services.RegisterControllerModule();
        services.RegisterWorldModule();
        services.RegisterDatabaseModule(connectionString);

        return services;
    }
}
