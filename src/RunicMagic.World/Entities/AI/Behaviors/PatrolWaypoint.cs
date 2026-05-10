using RunicMagic.World.Geometry;

namespace RunicMagic.World.Entities.AI.Behaviors;

public record PatrolWaypoint(Location Location, long WaitTicks, double? Facing);
