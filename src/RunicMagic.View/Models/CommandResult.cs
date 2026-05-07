namespace RunicMagic.View.Models;

public record CommandResult(
    IReadOnlyList<string> Text,
    IReadOnlyList<EntityRenderingModel> Entities,
    string Prompt);
