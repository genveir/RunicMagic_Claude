using RunicMagic.World.Entities;

namespace RunicMagic.World.Engine;

public readonly record struct EntitySetSelectionResult(IReadOnlyList<Entity> Entities, long FictionalResults);
