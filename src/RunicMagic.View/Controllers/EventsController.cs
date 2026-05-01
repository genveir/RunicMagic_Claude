using Microsoft.AspNetCore.Mvc;
using RunicMagic.View.Services;
using System.Text.Json;

namespace RunicMagic.View.Controllers;

[ApiController]
[Route("[controller]")]
public class EventsController(SseConnectionManager sseManager) : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [HttpGet]
    public async Task Get(CancellationToken ct)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";

        var (id, channel) = sseManager.AddConnection("no caster >");
        try
        {
            await foreach (var result in channel.Reader.ReadAllAsync(ct))
            {
                var json = JsonSerializer.Serialize(result, JsonOptions);
                await Response.WriteAsync($"data: {json}\n\n", ct);
                await Response.Body.FlushAsync(ct);
            }
        }
        catch (OperationCanceledException)
        {
            // client disconnected
        }
        finally
        {
            sseManager.RemoveConnection(id);
        }
    }
}
