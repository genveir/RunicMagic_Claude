namespace RunicMagic.View.Models;

public record TickResult(
    IReadOnlyList<string> Text,
    IReadOnlyList<EntityRenderingModel> Entities,
    CasterDataModel? CasterData);
