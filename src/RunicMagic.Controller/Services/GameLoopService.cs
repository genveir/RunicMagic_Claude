using RunicMagic.Controller.Abstractions;
using RunicMagic.View.Abstractions;
using RunicMagic.View.Models;
using RunicMagic.World.Abstractions;
using RunicMagic.World.Entities;

namespace RunicMagic.Controller.Services;

internal class GameLoopService(
    IPlayerGameLoopInterface playerService,
    IGameLoopWorldModel world,
    IWorldTicker worldTicker,
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

        worldTicker.HandleTick(eventTracker, _currentTick);
        _currentTick++;

        var casterId = playerService.GetCasterId();

        var result = ToTickResult(eventTracker, casterId);

        if (result != null)
            sink.Push(result);
    }

    private TickResult? ToTickResult(EventTracker eventTracker, EntityId? casterId)
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
        var casterData = BuildCasterData(casterId);

        return new TickResult(text, renderingModels, casterData);
    }

    private CasterDataModel? BuildCasterData(EntityId? casterId)
    {
        if (casterId == null)
            return null;

        var caster = world.Find(casterId.Value);
        if (caster == null)
            return null;

        var currentHitPoints = caster.Life?.CurrentHitPoints;
        var maxHitPoints = caster.Life?.MaxHitPoints;
        var currentPower = caster.Reservoir?.GetCurrentIncludingScope(caster);
        var maxPower = caster.Reservoir?.GetMaxIncludingScope(caster);

        return new CasterDataModel(
            currentHitPoints,
            maxHitPoints,
            caster.StructuralIntegrity.CurrentIntegrity,
            caster.StructuralIntegrity.MaxIntegrity,
            currentPower,
            maxPower);
    }
}
