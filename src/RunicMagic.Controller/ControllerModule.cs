using Microsoft.Extensions.DependencyInjection;
using RunicMagic.Controller.Abstractions;
using RunicMagic.Controller.Services;

namespace RunicMagic.Controller
{
    public static class ControllerModule
    {
        public static IServiceCollection RegisterControllerModule(this IServiceCollection services)
        {
            services.AddSingleton<PlayerService>();
            services.AddSingleton<IPlayerViewInterface>(svc => svc.GetRequiredService<PlayerService>());
            services.AddSingleton<IPlayerOutputSink>(svc => svc.GetRequiredService<PlayerService>());

            services.AddSingleton<EntityFactory>();
            services.AddSingleton<WorldLoadingService>();

            return services;
        }
    }
}
