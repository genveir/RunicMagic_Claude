using Microsoft.Extensions.DependencyInjection;
using RunicMagic.Players.Abstractions;
using RunicMagic.Players.Services;

namespace RunicMagic.Players
{
    public static class PlayersModule
    {
        public static IServiceCollection RegisterPlayersModule(this IServiceCollection services)
        {
            services.AddSingleton<PlayerService>();
            services.AddSingleton<IPlayerViewInterface>(svc => svc.GetRequiredService<PlayerService>());

            return services;
        }
    }
}
