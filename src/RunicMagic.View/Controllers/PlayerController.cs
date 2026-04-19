using Microsoft.AspNetCore.Mvc;
using RunicMagic.Controller.Abstractions;
using RunicMagic.Controller.Models;

namespace RunicMagic.View.Controllers;

[ApiController]
public class PlayerController : ControllerBase
{
    private readonly IPlayerViewInterface playerViewInterface;

    public PlayerController(IPlayerViewInterface playerViewInterface)
    {
        this.playerViewInterface = playerViewInterface;
    }

    [HttpPost("pick-caster")]
    public async Task<CommandResult> PickCaster([FromBody] CanvasClickRequest request)
    {
        var worldCoordinate = new WorldCoordinate(request.X, request.Y);

        var commandResult = await playerViewInterface.SetCaster(worldCoordinate);

        return commandResult;
    }

    [HttpPost("move-caster")]
    public async Task<CommandResult> MoveCaster([FromBody] CanvasClickRequest request)
    {
        var worldCoordinate = new WorldCoordinate(request.X, request.Y);

        var commandResult = await playerViewInterface.MoveCaster(worldCoordinate);

        return commandResult;
    }

    [HttpPost("point-at")]
    public async Task<CommandResult> PointAt([FromBody] CanvasClickRequest request)
    {
        var worldCoordinate = new WorldCoordinate(request.X, request.Y);

        var commandResult = await playerViewInterface.SetPointingDirection(worldCoordinate);

        return commandResult;
    }

    [HttpPost("indicate")]
    public async Task<CommandResult> Indicate([FromBody] CanvasClickRequest request)
    {
        var worldCoordinate = new WorldCoordinate(request.X, request.Y);

        var commandResult = await playerViewInterface.SetIndicateTarget(worldCoordinate);

        return commandResult;
    }
}

public record CanvasClickRequest(double X, double Y);
