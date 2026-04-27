using System.Collections.Concurrent;
using System.Threading.Channels;
using RunicMagic.Controller.Abstractions;
using RunicMagic.Controller.Models;
using RunicMagic.Controller.Services;

namespace RunicMagic.View.Services;

public class SseConnectionManager(WorldRenderingService worldRendering) : IWorldTickSink
{
    private readonly ConcurrentDictionary<Guid, Channel<CommandResult>> _connections = new();

    public (Guid Id, Channel<CommandResult> Channel) AddConnection(string initialPrompt)
    {
        var id = Guid.NewGuid();
        var channel = Channel.CreateUnbounded<CommandResult>();
        _connections[id] = channel;

        var entities = worldRendering.GetAllRenderingModels(casterEntityId: null);
        var initial = new CommandResult([], entities, initialPrompt);
        channel.Writer.TryWrite(initial);

        return (id, channel);
    }

    public void RemoveConnection(Guid id)
    {
        if (_connections.TryRemove(id, out var channel))
            channel.Writer.TryComplete();
    }

    public void Push(CommandResult result)
    {
        foreach (var channel in _connections.Values)
            channel.Writer.TryWrite(result);
    }
}
