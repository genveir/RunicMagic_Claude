namespace RunicMagic.View.Models;

public record ViewUpdateModel(
    IReadOnlyList<string> Text,
    IReadOnlyList<EntityRenderingModel> Entities,
    string Prompt,
    CasterBarsModel? Bars);

