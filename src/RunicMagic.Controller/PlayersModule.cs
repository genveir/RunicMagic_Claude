using Microsoft.Extensions.DependencyInjection;
using RunicMagic.Controller.Abstractions;
using RunicMagic.Controller.Services;

namespace RunicMagic.Controller
{
    public static class PlayersModule
    {
        public static IServiceCollection RegisterPlayersModule(this IServiceCollection services)
        {
            services.AddSingleton<PlayerService>();
            services.AddSingleton<IPlayerViewInterface>(svc => svc.GetRequiredService<PlayerService>());
            services.AddSingleton<IPlayerOutputSink>(svc => svc.GetRequiredService<PlayerService>());

            return services;
        }
    }
}
