using Microsoft.AspNetCore.Mvc;
using RunicMagic.Controller.Models;
using RunicMagic.Controller.Services;

namespace RunicMagic.View.Controllers;

[ApiController]
[Route("[controller]")]
public class WorldController(WorldRenderingService worldRendering) : ControllerBase
{
    [HttpGet]
    public IReadOnlyList<EntityRenderingModel> Get()
    {
        var models = worldRendering.GetAllRenderingModels();
        return models;
    }
}
