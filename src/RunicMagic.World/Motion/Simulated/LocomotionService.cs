using RunicMagic.World.Entities;
using RunicMagic.World.Geometry;
using RunicMagic.World.Motion.Shared;

namespace RunicMagic.World.Motion.Simulated;

public static class LocomotionService
{
    private const long AccelerationPerTick = 1;

    public static bool TryMove(Entity entity, Direction direction, IWorldEventTracker tracker)
    {
        if (entity.IsUnderEngineMotion)
        {
            return false;
        }

        if (entity.Locomotion == null)
        {
            return false;
        }

        entity.Locomotion.CurrentSpeed += AccelerationPerTick;

        var destination = entity.Location.Translate(direction, entity.Locomotion.CurrentSpeed);
        MoveEntityService.Move(entity, destination, entity.Angle, tracker);
        return true;
    }
}
