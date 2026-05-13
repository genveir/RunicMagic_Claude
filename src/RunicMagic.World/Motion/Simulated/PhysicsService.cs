using RunicMagic.World.Entities;
using RunicMagic.World.Geometry;
using RunicMagic.World.Motion.Shared;

namespace RunicMagic.World.Motion.Simulated;

public static class PhysicsService
{
    private const double MinSpeedMmPerTick = 1.0;
    private const double MinAngularSpeedRadPerTick = 0.001;

    public static void Tick(IEnumerable<Entity> entities, IWorldEventTracker tracker)
    {
        foreach (var entity in entities)
        {
            TickEntity(entity, tracker);
        }
    }

    private static void TickEntity(Entity entity, IWorldEventTracker tracker)
    {
        var (fx, fy, torque) = CalculateNetForceAndTorque(entity.PendingImpulses);
        entity.PendingImpulses.Clear();

        if (entity.IsUnderEngineMotion)
        {
            return;
        }

        var initialVelocity = entity.Velocity;

        if ((initialVelocity.Vx == 0 && initialVelocity.Vy == 0 && initialVelocity.Omega == 0) && fx == 0 && fy == 0 && torque == 0)
        {
            return;
        }

        var bounds = entity.Bounds;
        var (vx, vy) = CalculateLinearVelocity(
            initialVelocity: initialVelocity,
            weight: entity.Weight,
            dragCoefficient: entity.DragCoefficient,
            bounds: bounds,
            fx: fx,
            fy: fy,
            groundFrictionCoefficient: entity.GroundFrictionCoefficient,
            isGrounded: entity.IsGrounded);
        var linearStopped = vx == 0.0 && vy == 0.0;

        var omega = CalculateAngularVelocity(
            initialVelocity: initialVelocity,
            weight: entity.Weight,
            height: entity.Height,
            width: entity.Width,
            angularDragCoefficient: entity.AngularDragCoefficient,
            torque: torque,
            groundFrictionCoefficient: entity.GroundFrictionCoefficient,
            groundContactRadius: entity.GroundContactRadius,
            isGrounded: entity.IsGrounded);
        var rotationStopped = omega == 0.0;

        if (linearStopped && rotationStopped)
        {
            entity.Velocity = VelocityVector.Zero;
            return;
        }

        entity.Velocity = new VelocityVector(vx, vy, omega);

        var newAngle = entity.FacingAngle + omega;
        var destination = new Location(
            X: entity.Location.X + vx,
            Y: entity.Location.Y + vy);
        MoveEntityService.Move(entity, destination, newAngle, tracker);
    }

    public static (double fX, double fY, double torque) CalculateNetForceAndTorque(IReadOnlyList<ForceVector> pendingImpulses)
    {
        var fx = 0.0;
        var fy = 0.0;
        var torque = 0.0;
        foreach (var impulse in pendingImpulses)
        {
            fx += impulse.Fx;
            fy += impulse.Fy;
            torque += impulse.Rx * impulse.Fy - impulse.Ry * impulse.Fx;
        }

        return (fx, fy, torque);
    }

    public static (double vX, double vY) CalculateLinearVelocity(VelocityVector? initialVelocity, double weight, double dragCoefficient, Rectangle bounds, double fx, double fy, double groundFrictionCoefficient, bool isGrounded)
    {
        if (weight == 0.0)
        {
            weight = 0.00000000001;
        }

        var vx = initialVelocity?.Vx ?? 0.0;
        var vy = initialVelocity?.Vy ?? 0.0;

        vx += fx / weight;
        vy += fy / weight;

        var speed = Math.Sqrt(vx * vx + vy * vy);

        if (speed > 0.0 && dragCoefficient > 0.0)
        {
            var motionDirection = new Direction(vx / speed, vy / speed);
            var projectedWidth = bounds.GetProjectedWidth(motionDirection);

            var dragForceMagnitude = dragCoefficient * projectedWidth * speed * speed;
            var dragDeceleration = dragForceMagnitude / weight;
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

        if (isGrounded && groundFrictionCoefficient > 0.0 && speed > 0.0)
        {
            var newSpeed = Math.Max(0.0, speed - groundFrictionCoefficient);
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

        var linearStopped = speed < MinSpeedMmPerTick;

        if (linearStopped)
        {
            vx = 0.0;
            vy = 0.0;
        }

        return (vx, vy);
    }

    public static double CalculateAngularVelocity(VelocityVector? initialVelocity, double weight, double height, double width, double angularDragCoefficient, double torque, double groundFrictionCoefficient, double groundContactRadius, bool isGrounded)
    {
        if (weight == 0.0)
        {
            weight = 0.00000000001;
        }

        var omega = initialVelocity?.Omega ?? 0.0;

        var momentOfInertia = weight * (width * width + height * height) / 12.0;
        omega += torque / momentOfInertia;

        var absOmega = Math.Abs(omega);
        if (absOmega > 0.0 && angularDragCoefficient > 0.0)
        {
            var angularDragTorque = angularDragCoefficient * absOmega * absOmega;
            var angularDragDecel = angularDragTorque / momentOfInertia;
            var newAbsOmega = Math.Max(0.0, absOmega - angularDragDecel);
            omega = omega >= 0.0 ? newAbsOmega : -newAbsOmega;
            absOmega = newAbsOmega;
        }

        if (isGrounded && groundFrictionCoefficient > 0.0 && absOmega > 0.0)
        {
            var frictionTorque = groundFrictionCoefficient * weight * groundContactRadius;
            var frictionDecel = frictionTorque / momentOfInertia;
            var newAbsOmega = Math.Max(0.0, absOmega - frictionDecel);
            omega = omega >= 0.0 ? newAbsOmega : -newAbsOmega;
            absOmega = newAbsOmega;
        }

        var rotationStopped = absOmega < MinAngularSpeedRadPerTick;

        if (rotationStopped)
        {
            omega = 0.0;
        }

        return omega;
    }
}
