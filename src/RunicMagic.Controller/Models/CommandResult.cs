namespace RunicMagic.Controller.Models;

public record CommandResult(
    IReadOnlyList<string> Text,
    IReadOnlyList<EntityRenderingModel> Entities);
