using Microsoft.Extensions.Hosting;
using RunicMagic.Controller.Abstractions;
using RunicMagic.World;

namespace RunicMagic.Controller.Services;

internal class GameLoopService(PlayerService playerService, WorldModel world, IWorldTickSink sink) : BackgroundService
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
        var motionResult = world.TickMotion();
        playerService.ReceiveMotionEvents(motionResult);
        var result = playerService.DrainAndFlush();
        if (result != null)
            sink.Push(result);
    }
}
