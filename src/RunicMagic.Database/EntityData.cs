namespace RunicMagic.Database;

public record EntityData(
    Guid Id,
    int TypeId,
    string Label,
    int X,
    int Y,
    int Width,
    int Height,
    bool HasAgency,
    int? MaxHitPoints = null,
    int? CurrentHitPoints = null,
    int? MaxCharge = null,
    int? CurrentCharge = null);
