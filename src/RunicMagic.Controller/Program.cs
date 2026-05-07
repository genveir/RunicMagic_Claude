using RunicMagic.Controller.Services;
using RunicMagic.View;

namespace RunicMagic.Controller;

public class Program
{
    public static async Task Main(string[] args)
    {
        await ViewHost.RunAsync(
            args,
            configure: (config, services) =>
            {
                var connectionString = config.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

                services.RegisterApplicationModules(connectionString);
            },
            onBeforeRun: async services =>
            {
                await services.GetRequiredService<WorldLoadingService>().LoadAsync();
            });
    }
}
