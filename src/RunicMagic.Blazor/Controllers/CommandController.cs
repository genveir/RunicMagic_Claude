using Microsoft.AspNetCore.Mvc;
using RunicMagic.Players.Abstractions;
using RunicMagic.Players.Models;

namespace RunicMagic.Blazor.Controllers;

[ApiController]
[Route("[controller]")]
public class CommandController(IPlayerViewInterface player) : ControllerBase
{
    [HttpPost]
    public async Task<CommandResult> Post([FromBody] string input) =>
        await player.RegisterInput(input);
}
