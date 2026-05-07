using Microsoft.AspNetCore.Mvc;
using RunicMagic.View.Abstractions;

namespace RunicMagic.View.Controllers;

[ApiController]
[Route("[controller]")]
public class CommandController(IPlayerViewInterface player) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] string input)
    {
        await player.RegisterInput(input);

        return NoContent();
    }
}
