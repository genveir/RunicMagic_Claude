using Microsoft.AspNetCore.Mvc;
using RunicMagic.View.Abstractions;
using RunicMagic.View.Models;

namespace RunicMagic.View.Controllers;

[ApiController]
public class PlayerController : ControllerBase
{
    private readonly IPlayerViewInterface player;

    public PlayerController(IPlayerViewInterface player)
    {
        this.player = player;
    }

    [HttpPost("pick-caster")]
    public async Task<IActionResult> PickCaster([FromBody] CanvasClickRequest request)
    {
        await player.SetCaster(new WorldCoordinate(request.X, request.Y));

        return NoContent();
    }

    [HttpPost("move-caster")]
    public async Task<IActionResult> MoveCaster([FromBody] CanvasClickRequest request)
    {
        await player.MoveCaster(new WorldCoordinate(request.X, request.Y));

        return NoContent();
    }

    [HttpPost("point-at")]
    public async Task<IActionResult> PointAt([FromBody] CanvasClickRequest request)
    {
        await player.SetPointingDirection(new WorldCoordinate(request.X, request.Y));

        return NoContent();
    }

    [HttpPost("indicate")]
    public async Task<IActionResult> Indicate([FromBody] CanvasClickRequest request)
    {
        await player.SetIndicateTarget(new WorldCoordinate(request.X, request.Y));

        return NoContent();
    }
}

public record CanvasClickRequest(double X, double Y);
