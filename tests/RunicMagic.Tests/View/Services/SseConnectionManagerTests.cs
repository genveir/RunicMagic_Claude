using RunicMagic.Controller.Services;
using RunicMagic.View.Models;
using RunicMagic.View.Services;
using RunicMagic.World;
using RunicMagic.World.Geometry;

namespace RunicMagic.Tests.View.Services;

public class SseConnectionManagerTests
{
    private static SseConnectionManager MakeManager()
    {
        var world = new WorldModel();
        world.Add(new EntityBuilder().WithLocation(x: 1000, y: 1000).Build());
        var worldRendering = new WorldRenderingService(world, new RayCastService(world));
        return new SseConnectionManager(worldRendering);
    }

    [Fact]
    public void AddConnection_SendsInitialStateImmediatelyToChannel()
    {
        var manager = MakeManager();

        var (_, channel) = manager.AddConnection();

        channel.Reader.TryRead(out var result).Should().BeTrue();
        result!.Entities.Count.Should().Be(1);
    }

    [Fact]
    public void Push_SingleConnection_ChannelReceivesResult()
    {
        var manager = MakeManager();
        var (_, channel) = manager.AddConnection();
        channel.Reader.TryRead(out _);

        var pushed = new ViewUpdateModel(["hello"], [], "prompt");
        manager.Push(pushed);

        channel.Reader.TryRead(out var received).Should().BeTrue();
        received.Should().BeEquivalentTo(pushed);
    }

    [Fact]
    public void Push_MultipleConnections_AllChannelsReceiveResult()
    {
        var manager = MakeManager();
        var (_, channel1) = manager.AddConnection();
        var (_, channel2) = manager.AddConnection();
        channel1.Reader.TryRead(out _);
        channel2.Reader.TryRead(out _);

        var pushed = new ViewUpdateModel(["hello"], [], "prompt");
        manager.Push(pushed);

        channel1.Reader.TryRead(out var r1).Should().BeTrue();
        channel2.Reader.TryRead(out var r2).Should().BeTrue();
        r1.Should().BeEquivalentTo(pushed);
        r2.Should().BeEquivalentTo(pushed);
    }

    [Fact]
    public void RemoveConnection_CompletesChannel()
    {
        var manager = MakeManager();
        var (id, channel) = manager.AddConnection();
        channel.Reader.TryRead(out _);

        manager.RemoveConnection(id);

        channel.Reader.Completion.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void Push_AfterRemoveConnection_RemovedChannelReceivesNothing()
    {
        var manager = MakeManager();
        var (id, channel) = manager.AddConnection();
        channel.Reader.TryRead(out _);
        manager.RemoveConnection(id);

        manager.Push(new ViewUpdateModel(["hello"], [], "prompt"));

        channel.Reader.TryRead(out _).Should().BeFalse();
    }
}
