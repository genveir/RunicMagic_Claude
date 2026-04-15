using RunicMagic.Players;
using RunicMagic.Players.Abstractions;
using System.Text.Json;

namespace RunicMagic.Blazor;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.RegisterPlayersModule();

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

        var app = builder.Build();

        app.UseStaticFiles();
        app.MapControllers();
        app.MapFallbackToFile("index.html");

        app.Run();
    }
}