using RunicMagic.Controller.Abstractions;
using RunicMagic.Controller.Models;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.Services;
using RunicMagic.Tests.Builders;
using RunicMagic.World;
using RunicMagic.World.Abstractions;
using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;

namespace RunicMagic.Tests;

public class GameLoopServiceTests
{
    private static (GameLoopService loop, PlayerService playerService, CapturingSink sink) MakeComponents()
    {
        var world = new WorldModel();
        var spellCasting = new SpellCastingService(world, new SpellExecutor(world));
        var playerService = new PlayerService(world, spellCasting, new RayCastService(world));
        var sink = new CapturingSink();
        var loop = new GameLoopService(playerService, new FakeWorld(), new FakeRendering(), sink);
        return (loop, playerService, sink);
    }

    private static (GameLoopService loop, FakeWorld world) MakeLoopWithFakeWorld()
    {
        var world = new FakeWorld();
        var sink = new CapturingSink();
        var loop = new GameLoopService(new FakePlayerService(_ => { }), world, new FakeRendering(), sink);
        return (loop, world);
    }

    private class CapturingSink : IWorldTickSink
    {
        public List<CommandResult> Pushed { get; } = [];
        public void Push(CommandResult result) { Pushed.Add(result); }
    }

    private class FakePlayerService : IPlayerGameLoopInterface
    {
        private readonly Action<EventTracker> _drain;

        public FakePlayerService(Action<EventTracker> drain)
        {
            _drain = drain;
        }

        public EntityId? GetCasterId()
        {
            return null;
        }

        public void DrainAndFlush(EventTracker eventTracker)
        {
            _drain(eventTracker);
        }
    }

    private class FakeWorld : IGameLoopWorldModel
    {
        public Entity? FindResult { get; set; }

        public void TickMotion(IWorldEventTracker eventTracker) { }

        public void TickAI(IWorldEventTracker eventTracker) { }

        public Entity? Find(EntityId id)
        {
            return FindResult;
        }
    }

    private class FakeRendering : IWorldRenderingService
    {
        public IReadOnlyList<EntityRenderingModel> GetAllRenderingModels(EntityId? casterEntityId)
        {
            return [];
        }
    }

    private static (GameLoopService loop, CapturingSink sink) MakeLoopWithFake(Action<EventTracker> drain)
    {
        var sink = new CapturingSink();
        var loop = new GameLoopService(new FakePlayerService(drain), new FakeWorld(), new FakeRendering(), sink);
        return (loop, sink);
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
    public void Tick_WithWorldEvent_PushesResult()
    {
        var (loop, sink) = MakeLoopWithFake(et => et.Add(new DebugOutputEvent("test")));

        loop.Tick();

        sink.Pushed.Should().HaveCount(1);
    }

    [Fact]
    public void Tick_WithControllerEvent_PushesResult()
    {
        var (loop, sink) = MakeLoopWithFake(et => et.Add(new NoCasterSelectedEvent()));

        loop.Tick();

        sink.Pushed.Should().HaveCount(1);
    }

    [Fact]
    public void Tick_WithParseEvent_PushesResult()
    {
        var (loop, sink) = MakeLoopWithFake(et => et.Add(new RanOutOfTokensEvent()));

        loop.Tick();

        sink.Pushed.Should().HaveCount(1);
    }

    [Fact]
    public void Tick_WithTouchedEntity_PushesResult()
    {
        var entity = new EntityBuilder().WithLocation(x: 0, y: 0).Build();
        var (loop, sink) = MakeLoopWithFake(et => et.Track(entity));

        loop.Tick();

        sink.Pushed.Should().HaveCount(1);
    }

    [Fact]
    public void Tick_WithWorldEvent_ResultContainsEventText()
    {
        var (loop, sink) = MakeLoopWithFake(et => et.Add(new DebugOutputEvent("test")));

        loop.Tick();

        sink.Pushed[0].Text.Should().HaveCount(1);
    }

    [Fact]
    public void Tick_WithControllerEvent_ResultContainsEventText()
    {
        var (loop, sink) = MakeLoopWithFake(et => et.Add(new NoCasterSelectedEvent()));

        loop.Tick();

        sink.Pushed[0].Text.Should().HaveCount(1);
    }

    [Fact]
    public void Tick_WithParseEvent_ResultContainsEventText()
    {
        var (loop, sink) = MakeLoopWithFake(et => et.Add(new RanOutOfTokensEvent()));

        loop.Tick();

        sink.Pushed[0].Text.Should().HaveCount(1);
    }

    [Fact]
    public void Tick_WithAllEventTypes_ResultContainsTextForEachTextProducingEvent()
    {
        var entity = new EntityBuilder().WithLocation(x: 0, y: 0).Build();
        var (loop, sink) = MakeLoopWithFake(et =>
        {
            et.Add(new DebugOutputEvent("test"));
            et.Add(new NoCasterSelectedEvent());
            et.Add(new RanOutOfTokensEvent());
            et.Track(entity);
        });

        loop.Tick();

        sink.Pushed[0].Text.Should().HaveCount(3);
    }

    [Fact]
    public void GetPrompt_NoCasterSelected_ReturnsNoCasterPrompt()
    {
        var (loop, _) = MakeLoopWithFakeWorld();

        var prompt = loop.GetPrompt(casterId: null);

        prompt.Should().Be("[no caster] >");
    }

    [Fact]
    public void GetPrompt_CasterNotInWorld_ReturnsDeadCasterPrompt()
    {
        var (loop, _) = MakeLoopWithFakeWorld();

        var prompt = loop.GetPrompt(EntityId.New());

        prompt.Should().Be("[dead caster] >");
    }

    [Fact]
    public void GetPrompt_CasterWithoutLife_ReturnsDeadCasterPrompt()
    {
        var (loop, world) = MakeLoopWithFakeWorld();
        world.FindResult = new EntityBuilder()
            .WithLocation(x: 0, y: 0)
            .WithAgency()
            .Build();

        var prompt = loop.GetPrompt(EntityId.New());

        prompt.Should().Be("[dead caster] >");
    }

    [Fact]
    public void GetPrompt_CasterWithLife_ShowsHitPointsAndIntegrity()
    {
        var (loop, world) = MakeLoopWithFakeWorld();
        world.FindResult = new EntityBuilder()
            .WithLocation(x: 0, y: 0)
            .WithAgency()
            .WithLife(max: 20, current: 15)
            .Build();

        var prompt = loop.GetPrompt(EntityId.New());

        prompt.Should().Be("(15/20H) (1000/1000I) >");
    }
}
