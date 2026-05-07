using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace RunicMagic.View;

public static class ViewHost
{
    public static async Task RunAsync(
        string[] args,
        Action<IConfiguration, IServiceCollection> configure,
        Func<IServiceProvider, Task>? onBeforeRun = null)
    {
        var bootstrapConfig = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        var staticFilesPath = bootstrapConfig["StaticFiles:Path"]
            ?? throw new InvalidOperationException("StaticFiles:Path not configured.");

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            WebRootPath = staticFilesPath
        });

        builder.Host.UseSerilog((ctx, lc) => lc.WriteTo.Console());

        configure(builder.Configuration, builder.Services);

        var app = builder.Build();

        if (onBeforeRun != null)
            await onBeforeRun(app.Services);

        app.UseStaticFiles();
        app.MapControllers();
        app.MapFallbackToFile("index.html");

        await app.RunAsync();
    }
}
