using Microsoft.Extensions.DependencyInjection;

namespace RunicMagic.Database;

public static class DatabaseModule
{
    public static IServiceCollection RegisterDatabaseModule(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton(new WorldLoader(connectionString));
        return services;
    }
}
