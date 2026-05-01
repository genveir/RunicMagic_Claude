using RunicMagic.Controller.Abstractions;
using RunicMagic.Controller.Models;
using RunicMagic.Controller.Services;
using RunicMagic.Tests.Builders;
using RunicMagic.World;
using RunicMagic.World.Geometry;

namespace RunicMagic.Tests;

public class GameLoopServiceTests
{
    private static (GameLoopService loop, PlayerService playerService, CapturingSink sink) MakeComponents()
    {
        var world = new WorldModel();
        var worldRendering = new WorldRenderingService(world, new RayCastService(world));
        var spellCasting = new SpellCastingService(world, new SpellExecutor(world));
        var playerService = new PlayerService(world, spellCasting, new RayCastService(world));
        var sink = new CapturingSink();
        var loop = new GameLoopService(playerService, world, worldRendering, sink);
        return (loop, playerService, sink);
    }

    private static (GameLoopService loop, WorldModel world) MakeLoopWithWorld()
    {
        var world = new WorldModel();
        var worldRendering = new WorldRenderingService(world, new RayCastService(world));
        var spellCasting = new SpellCastingService(world, new SpellExecutor(world));
        var playerService = new PlayerService(world, spellCasting, new RayCastService(world));
        var sink = new CapturingSink();
        var loop = new GameLoopService(playerService, world, worldRendering, sink);
        return (loop, world);
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

    [Fact]
    public void GetPrompt_NoCasterSelected_ReturnsNoCasterPrompt()
    {
        var (loop, _) = MakeLoopWithWorld();

        var prompt = loop.GetPrompt(casterId: null);

        prompt.Should().Be("[no caster] >");
    }

    [Fact]
    public void GetPrompt_CasterNotInWorld_ReturnsDeadCasterPrompt()
    {
        var (loop, _) = MakeLoopWithWorld();

        var prompt = loop.GetPrompt(EntityId.New());

        prompt.Should().Be("[dead caster] >");
    }

    [Fact]
    public void GetPrompt_CasterWithoutLife_ReturnsDeadCasterPrompt()
    {
        var (loop, world) = MakeLoopWithWorld();
        var entity = new EntityBuilder()
            .WithLocation(x: 0, y: 0)
            .WithAgency()
            .Build();
        world.Add(entity);

        var prompt = loop.GetPrompt(entity.Id);

        prompt.Should().Be("[dead caster] >");
    }

    [Fact]
    public void GetPrompt_CasterWithLife_ShowsHitPointsAndIntegrity()
    {
        var (loop, world) = MakeLoopWithWorld();
        var entity = new EntityBuilder()
            .WithLocation(x: 0, y: 0)
            .WithAgency()
            .WithLife(max: 20, current: 15)
            .Build();
        world.Add(entity);

        var prompt = loop.GetPrompt(entity.Id);

        prompt.Should().Be("(15/20H) (1000/1000I) >");
    }
}
