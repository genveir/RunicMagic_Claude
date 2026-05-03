using RunicMagic.World.Entities;
using RunicMagic.World.Geometry;
using RunicMagic.World.Motion.Shared;

namespace RunicMagic.World.Motion.Simulated;

public static class PhysicsService
{
    private const double MinSpeedMmPerTick = 1.0;

    public static void Tick(IReadOnlyList<Entity> entities, IWorldEventTracker tracker)
    {
        foreach (var entity in entities)
        {
            TickEntity(entity, tracker);
        }
    }

    private static void TickEntity(Entity entity, IWorldEventTracker tracker)
    {
        if (entity.IsUnderEngineMotion)
        {
            entity.PendingImpulses.Clear();
            return;
        }

        if (entity.Weight == 0)
        {
            entity.PendingImpulses.Clear();
            return;
        }

        var fx = 0.0;
        var fy = 0.0;
        foreach (var impulse in entity.PendingImpulses)
        {
            fx += impulse.Fx;
            fy += impulse.Fy;
        }
        entity.PendingImpulses.Clear();

        var vx = entity.Velocity?.Vx ?? 0.0;
        var vy = entity.Velocity?.Vy ?? 0.0;

        vx += fx / entity.Weight;
        vy += fy / entity.Weight;

        var speed = Math.Sqrt(vx * vx + vy * vy);

        if (speed > 0.0 && entity.DragCoefficient > 0.0)
        {
            var bounds = new Rectangle(entity.Location, entity.Width, entity.Height, entity.Angle);
            var motionDirection = new Direction(vx / speed, vy / speed);
            var projectedWidth = bounds.GetProjectedWidth(motionDirection);

            var dragForceMagnitude = entity.DragCoefficient * projectedWidth * speed * speed;
            var dragDeceleration = dragForceMagnitude / entity.Weight;
            var newSpeed = Math.Max(0.0, speed - dragDeceleration);

            if (newSpeed > 0.0)
            {
                var scale = newSpeed / speed;
                vx *= scale;
                vy *= scale;
            }
            else
            {
                vx = 0.0;
                vy = 0.0;
            }

            speed = newSpeed;
        }

        if (speed < MinSpeedMmPerTick)
        {
            entity.Velocity = null;
            return;
        }

        entity.Velocity = new VelocityVector(vx, vy);

        var destination = new Location(entity.Location.X + vx, entity.Location.Y + vy);
        MoveEntityService.Move(entity, destination, entity.Angle, tracker);
    }
}
