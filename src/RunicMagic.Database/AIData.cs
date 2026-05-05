namespace RunicMagic.Database;

public record AIData(
    Guid EntityId,
    PatrolBehaviorData[]? PatrolBehaviors = null);
