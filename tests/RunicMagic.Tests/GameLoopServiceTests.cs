using FluentAssertions;
using RunicMagic.Controller.Abstractions;
using RunicMagic.Controller.Models;
using RunicMagic.Controller.Services;
using RunicMagic.World;
using RunicMagic.World.Geometry;
using Xunit;

namespace RunicMagic.Tests;

public class GameLoopServiceTests
{
    private static (GameLoopService loop, PlayerService playerService, CapturingSink sink) MakeComponents()
    {
        var world = new WorldModel();
        var worldRendering = new WorldRenderingService(world, new RayCastService(world));
        var spellCasting = new SpellCastingService(world, new SpellExecutor(world));
        var playerService = new PlayerService(world, worldRendering, spellCasting, new RayCastService(world));
        var sink = new CapturingSink();
        var loop = new GameLoopService(playerService, world, sink);
        return (loop, playerService, sink);
    }

    private class CapturingSink : IWorldTickSink
    {
        public List<CommandResult> Pushed { get; } = [];
        public void Push(CommandResult result) { Pushed.Add(result); }
    }

    [Fact]
    public void Tick_EmptyQueue_DoesNotPushToSink()
    {
        var (loop, _, sink) = MakeComponents();

        loop.Tick();

        sink.Pushed.Should().BeEmpty();
    }

    [Fact]
    public async Task Tick_QueueHasItems_PushesResultToSink()
    {
        var (loop, playerService, sink) = MakeComponents();
        await playerService.RegisterInput("ZU VUN LA TOT");

        loop.Tick();

        sink.Pushed.Should().HaveCount(1);
    }

    [Fact]
    public async Task Tick_QueueHasItems_PushedResultContainsQueuedText()
    {
        var (loop, playerService, sink) = MakeComponents();
        await playerService.RegisterInput("ZU VUN LA TOT");

        loop.Tick();

        sink.Pushed[0].Text.Should().Contain(l => l.Contains("No caster selected"));
    }

    [Fact]
    public async Task Tick_AfterDraining_SubsequentEmptyTickDoesNotPush()
    {
        var (loop, playerService, sink) = MakeComponents();
        await playerService.RegisterInput("ZU VUN LA TOT");
        loop.Tick();

        loop.Tick();

        sink.Pushed.Should().HaveCount(1);
    }
}
