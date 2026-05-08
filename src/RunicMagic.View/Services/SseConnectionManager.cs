using RunicMagic.View.Abstractions;
using RunicMagic.View.Models;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace RunicMagic.View.Services;

public class SseConnectionManager(IWorldRenderingService worldRendering)
{
    private readonly ConcurrentDictionary<Guid, Channel<ViewUpdateModel>> _connections = new();

    public (Guid Id, Channel<ViewUpdateModel> Channel) AddConnection()
    {
        var id = Guid.NewGuid();
        var channel = Channel.CreateUnbounded<ViewUpdateModel>();
        _connections[id] = channel;

        var entities = worldRendering.GetAllRenderingModels(casterEntityId: null);
        var initial = new ViewUpdateModel([], entities, string.Empty);
        channel.Writer.TryWrite(initial);

        return (id, channel);
    }

    public void RemoveConnection(Guid id)
    {
        if (_connections.TryRemove(id, out var channel))
            channel.Writer.TryComplete();
    }

    public void Push(ViewUpdateModel viewUpdate)
    {
        foreach (var channel in _connections.Values)
            channel.Writer.TryWrite(viewUpdate);
    }
}
