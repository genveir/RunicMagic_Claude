namespace RunicMagic.Database;

public record EntityData(
    Guid Id,
    long TypeId,
    string Label,
    long X,
    long Y,
    long Width,
    long Height,
    bool HasAgency,
    long Weight,
    bool IsTranslucent = false,
    long? MaxHitPoints = null,
    long? CurrentHitPoints = null,
    long? MaxCharge = null,
    long? CurrentCharge = null,
    string[]? InscriptionTexts = null);
