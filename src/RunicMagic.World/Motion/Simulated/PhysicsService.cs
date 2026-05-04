using RunicMagic.World.Entities;
using RunicMagic.World.Geometry;
using RunicMagic.World.Motion.Shared;

namespace RunicMagic.World.Motion.Simulated;

public static class PhysicsService
{
    private const double MinSpeedMmPerTick = 1.0;
    private const double MinAngularSpeedRadPerTick = 0.001;

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
        var torque = 0.0;
        foreach (var impulse in entity.PendingImpulses)
        {
            fx += impulse.Fx;
            fy += impulse.Fy;
            torque += impulse.Rx * impulse.Fy - impulse.Ry * impulse.Fx;
        }
        entity.PendingImpulses.Clear();

        var vx = entity.Velocity?.Vx ?? 0.0;
        var vy = entity.Velocity?.Vy ?? 0.0;
        var omega = entity.Velocity?.Omega ?? 0.0;

        vx += fx / entity.Weight;
        vy += fy / entity.Weight;

        var momentOfInertia = (double)entity.Weight * ((double)entity.Width * entity.Width + (double)entity.Height * entity.Height) / 12.0;
        omega += torque / momentOfInertia;

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

        var absOmega = Math.Abs(omega);
        if (absOmega > 0.0 && entity.AngularDragCoefficient > 0.0)
        {
            var angularDragTorque = entity.AngularDragCoefficient * absOmega * absOmega;
            var angularDragDecel = angularDragTorque / momentOfInertia;
            var newAbsOmega = Math.Max(0.0, absOmega - angularDragDecel);
            omega = omega >= 0.0 ? newAbsOmega : -newAbsOmega;
            absOmega = newAbsOmega;
        }

        var linearStopped = speed < MinSpeedMmPerTick;
        var rotationStopped = absOmega < MinAngularSpeedRadPerTick;

        if (linearStopped && rotationStopped)
        {
            entity.Velocity = null;
            return;
        }

        if (linearStopped)
        {
            vx = 0.0;
            vy = 0.0;
        }

        if (rotationStopped)
        {
            omega = 0.0;
        }

        entity.Velocity = new VelocityVector(vx, vy, omega);

        var newAngle = entity.Angle + omega;
        var destination = new Location(entity.Location.X + vx, entity.Location.Y + vy);
        MoveEntityService.Move(entity, destination, newAngle, tracker);
    }
}
