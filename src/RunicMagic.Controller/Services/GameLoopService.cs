using RunicMagic.Controller.Abstractions;
using RunicMagic.View.Abstractions;
using RunicMagic.View.Models;
using RunicMagic.World.Abstractions;
using RunicMagic.World.Entities;

namespace RunicMagic.Controller.Services;

internal class GameLoopService(
    IPlayerGameLoopInterface playerService,
    IGameLoopWorldModel world,
    IWorldRenderingService worldRendering,
    IWorldTickSink sink) : BackgroundService
{
    private long _currentTick = 0;

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

        playerService.DrainAndFlush(eventTracker);

        world.HandleTick(eventTracker, _currentTick);
        _currentTick++;

        var casterId = playerService.GetCasterId();

        var result = ToCommandResult(eventTracker, casterId);

        if (result != null)
            sink.Push(result);
    }

    private CommandResult? ToCommandResult(EventTracker eventTracker, EntityId? casterId)
    {
        if (!eventTracker.HasTrackedChanges)
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

        var renderingModels = worldRendering.GetAllRenderingModels(casterId?.Value);

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
