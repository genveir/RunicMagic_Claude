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
    double Angle = 0,
    long? MaxHitPoints = null,
    long? CurrentHitPoints = null,
    long? MaxCharge = null,
    long? CurrentCharge = null,
    string[]? InscriptionTexts = null);
