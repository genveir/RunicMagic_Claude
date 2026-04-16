using Microsoft.AspNetCore.Mvc;
using RunicMagic.Controller.Abstractions;
using RunicMagic.Controller.Models;

namespace RunicMagic.View.Controllers;

[ApiController]
[Route("[controller]")]
public class CommandController(IPlayerViewInterface player) : ControllerBase
{
    [HttpPost]
    public async Task<CommandResult> Post([FromBody] string input)
    {
        var result = await player.RegisterInput(input);

        return result;
    }
}
