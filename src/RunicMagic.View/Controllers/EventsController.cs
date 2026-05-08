using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RunicMagic.View.Services;
using System.Text.Json;

namespace RunicMagic.View.Controllers;

[ApiController]
[Route("[controller]")]
public class EventsController : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly SseConnectionManager sseManager;

    public EventsController(SseConnectionManager sseManager)
    {
        this.sseManager = sseManager;
    }

    [HttpGet]
    public async Task Get(CancellationToken ct)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";

        var (id, channel) = sseManager.AddConnection();
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
