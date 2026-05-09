using RunicMagic.World.Entities;
using RunicMagic.World.Geometry;

namespace RunicMagic.World.Motion.Shared;

internal static class MoveEntityService
{
    public static void Move(Entity entity, Location location, double angle, IWorldEventTracker tracker)
    {
        entity.Location = location;
        entity.FacingAngle = angle;

        tracker.Track(entity);
    }
}
