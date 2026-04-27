using Microsoft.Extensions.Hosting;
using RunicMagic.Controller.Abstractions;

namespace RunicMagic.Controller.Services;

internal class GameLoopService(PlayerService playerService, IWorldTickSink sink) : BackgroundService
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
        var result = playerService.DrainAndFlush();
        if (result != null)
            sink.Push(result);
    }
}
