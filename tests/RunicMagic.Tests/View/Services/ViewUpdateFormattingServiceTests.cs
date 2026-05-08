using RunicMagic.Controller.Services;
using RunicMagic.View.Models;
using RunicMagic.View.Services;
using RunicMagic.World;
using RunicMagic.World.Geometry;

namespace RunicMagic.Tests.View.Services;

public class ViewUpdateFormattingServiceTests
{
    private static (ViewUpdateFormattingService service, SseConnectionManager manager) MakeComponents()
    {
        var world = new WorldModel();
        var worldRendering = new WorldRenderingService(world, new RayCastService(world));
        var manager = new SseConnectionManager(worldRendering);
        var service = new ViewUpdateFormattingService(manager);
        return (service, manager);
    }

    private static string PushAndReadPrompt(ViewUpdateFormattingService service, SseConnectionManager manager, CasterDataModel? casterData)
    {
        var (_, channel) = manager.AddConnection();
        channel.Reader.TryRead(out _);

        service.Push(new TickResult([], [], casterData));

        channel.Reader.TryRead(out var received);
        return received!.Prompt;
    }

    [Fact]
    public void Push_NoCasterData_PromptIsNoCaster()
    {
        var (service, manager) = MakeComponents();

        var prompt = PushAndReadPrompt(service, manager, casterData: null);

        prompt.Should().Be("[no caster] >");
    }

    [Fact]
    public void Push_CasterWithLifeAndIntegrity_PromptShowsBoth()
    {
        var (service, manager) = MakeComponents();
        var casterData = new CasterDataModel(
            CurrentHitPoints: 15,
            MaxHitPoints: 20,
            CurrentIntegrity: 1000,
            MaxIntegrity: 1000,
            CurrentPower: null,
            MaxPower: null);

        var prompt = PushAndReadPrompt(service, manager, casterData);

        prompt.Should().Be("(15/20H) (1000/1000I) >");
    }

    [Fact]
    public void Push_CasterWithLifeIntegrityAndPower_PromptShowsAll()
    {
        var (service, manager) = MakeComponents();
        var casterData = new CasterDataModel(
            CurrentHitPoints: 15,
            MaxHitPoints: 20,
            CurrentIntegrity: 1000,
            MaxIntegrity: 1000,
            CurrentPower: 30,
            MaxPower: 100);

        var prompt = PushAndReadPrompt(service, manager, casterData);

        prompt.Should().Be("(15/20H) (1000/1000I) (30/100P) >");
    }

    [Fact]
    public void Push_CasterWithZeroHitPoints_PromptIsDeadCaster()
    {
        var (service, manager) = MakeComponents();
        var casterData = new CasterDataModel(
            CurrentHitPoints: 0,
            MaxHitPoints: 20,
            CurrentIntegrity: 1000,
            MaxIntegrity: 1000,
            CurrentPower: null,
            MaxPower: null);

        var prompt = PushAndReadPrompt(service, manager, casterData);

        prompt.Should().Be("[dead caster] >");
    }

    [Fact]
    public void Push_CasterWithNoLife_PromptShowsIntegrityOnly()
    {
        var (service, manager) = MakeComponents();
        var casterData = new CasterDataModel(
            CurrentHitPoints: null,
            MaxHitPoints: null,
            CurrentIntegrity: 1000,
            MaxIntegrity: 1000,
            CurrentPower: null,
            MaxPower: null);

        var prompt = PushAndReadPrompt(service, manager, casterData);

        prompt.Should().Be("(1000/1000I) >");
    }

    [Fact]
    public void Push_ForwardsTextAndEntitiesToSse()
    {
        var (service, manager) = MakeComponents();
        var (_, channel) = manager.AddConnection();
        channel.Reader.TryRead(out _);

        service.Push(new TickResult(["line one", "line two"], [], CasterData: null));

        channel.Reader.TryRead(out var received);
        received!.Text.Should().BeEquivalentTo(["line one", "line two"]);
    }
}
