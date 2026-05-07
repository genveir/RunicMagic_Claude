using Microsoft.Extensions.DependencyInjection;
using RunicMagic.View.Abstractions;
using RunicMagic.View.Services;

namespace RunicMagic.View;

public static class ViewModule
{
    public static IServiceCollection RegisterViewModule(this IServiceCollection services)
    {
        services.AddSingleton<SseConnectionManager>();
        services.AddSingleton<IWorldTickSink>(svc => svc.GetRequiredService<SseConnectionManager>());

        services.AddControllers();

        return services;
    }
}
