using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;

namespace RunicMagic.World.Services;

internal static class MoveEntityService
{
    public static void Move(Entity entity, Location location, double angle, IWorldEventTracker tracker)
    {
        entity.Location = location;
        entity.Angle = angle;

        tracker.Track(entity);
    }
}
