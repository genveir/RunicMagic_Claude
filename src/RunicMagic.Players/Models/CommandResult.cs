namespace RunicMagic.Players.Models;

public record CommandResult(
    IReadOnlyList<string> Text,
    IReadOnlyList<EntityRenderingModel> Entities);
