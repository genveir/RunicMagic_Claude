using Microsoft.Extensions.Hosting;
using RunicMagic.Controller.Abstractions;
using RunicMagic.Controller.Models;
using RunicMagic.World;

namespace RunicMagic.Controller.Services;

internal class GameLoopService(PlayerService playerService, WorldModel world, WorldRenderingService worldRendering, IWorldTickSink sink) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(1000.0 / 60));
        while (await timer.WaitForNextTickAsync(ct))
        {
            Tick();
        }
    }

    internal void Tick()
    {
        var eventTracker = new EventTracker();

        world.TickMotion(eventTracker);

        var casterId = playerService.DrainAndFlush(eventTracker);

        var result = ToCommandResult(eventTracker, casterId);

        if (result != null)
            sink.Push(result);
    }

    private CommandResult? ToCommandResult(EventTracker eventTracker, EntityId? casterId)
    {
        if (eventTracker.TouchedEntities.Count + eventTracker.WorldEvents.Count + eventTracker.ControllerEvents.Count == 0)
            return null;

        List<string> text = [];
        foreach (var worldEvent in eventTracker.WorldEvents)
        {
            text.Add(EventDescriber.Describe(worldEvent));
        }

        foreach (var controllerEvent in eventTracker.ControllerEvents)
        {
            text.Add(EventDescriber.Describe(controllerEvent));
        }

        foreach (var parseEvent in eventTracker.ParseEvents)
        {
            text.Add(EventDescriber.Describe(parseEvent));
        }

        var renderingModels = worldRendering.GetAllRenderingModels(casterId);

        return new CommandResult(
            text,
            renderingModels,
            GetPrompt(casterId));
    }

    public string GetPrompt(EntityId? casterId)
    {
        if (casterId == null)
        {
            return "[no caster] >";
        }

        var caster = world.Find(casterId.Value);
        if (caster?.Life == null)
        {
            return "[dead caster] >";
        }

        var prompt = $"({caster.Life.CurrentHitPoints}/{caster.Life.MaxHitPoints}H) ({caster.StructuralIntegrity.CurrentIntegrity}/{caster.StructuralIntegrity.MaxIntegrity}I) >";
        return prompt;
    }
}