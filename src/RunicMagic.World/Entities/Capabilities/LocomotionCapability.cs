using RunicMagic.World.Geometry;
using RunicMagic.World.Motion.Simulated;

namespace RunicMagic.World.Entities.Capabilities;

public class LocomotionCapability
{
    private const int BrakeSearchSteps = 4;

    private readonly IReadOnlyList<Leg> legs;
    private readonly double locomotionEfficiency;

    public LocomotionCapability(IReadOnlyList<Leg> legs, double locomotionEfficiency)
    {
        this.legs = legs;
        this.locomotionEfficiency = locomotionEfficiency;
    }

    // --- Queries ---

    public IReadOnlyList<string> LegNames
    {
        get
        {
            var names = legs.Select(l => l.Name).ToList();
            return names;
        }
    }

    public (long Ticks, double Distance) GetLinearStopInfo(Entity entity, double forceFraction, int maxTicks = 10)
    {
        var facing = Direction.FromAngle(entity.FacingAngle);

        var perLegForce = (entity.Strength?.Value ?? 0.0) * locomotionEfficiency * forceFraction;
        var totalBrakingForce = legs.Count * perLegForce;

        var brakingFx = -facing.X * totalBrakingForce;
        var brakingFy = -facing.Y * totalBrakingForce;

        var result = SimulateLinearStop(
            vx: entity.Velocity.Vx,
            vy: entity.Velocity.Vy,
            brakingFx: brakingFx,
            brakingFy: brakingFy,
            weight: entity.Weight,
            dragCoefficient: entity.DragCoefficient,
            bounds: entity.Bounds,
            groundFrictionCoefficient: entity.GroundFrictionCoefficient,
            isGrounded: entity.IsGrounded,
            maxTicks: maxTicks);

        return result;
    }

    public (long Ticks, double Angle) GetAngularStopInfo(Entity entity, double forceFraction, int maxTicks = 10)
    {
        var facing = Direction.FromAngle(entity.FacingAngle);
        var perLegForce = (entity.Strength?.Value ?? 0.0) * locomotionEfficiency * forceFraction;

        var maxTurnForce = GetLegVectors(
            legs: legs,
            perLegForce: perLegForce,
            turnDirection: 1,
            entityAngle: entity.FacingAngle,
            scale: 1.0,
            facing: facing);
        var maxTurnTorque = PhysicsService.CalculateNetForceAndTorque(maxTurnForce).torque;

        if (maxTurnTorque == 0.0)
        {
            return (0L, 0.0);
        }

        var result = SimulateAngularStop(
            omega: entity.Velocity.Omega,
            maxCounterTorque: maxTurnTorque,
            weight: entity.Weight,
            width: entity.Width,
            height: entity.Height,
            angularDragCoefficient: entity.AngularDragCoefficient,
            groundFrictionCoefficient: entity.GroundFrictionCoefficient,
            groundContactRadius: entity.GroundContactRadius,
            isGrounded: entity.IsGrounded,
            maxTicks: maxTicks);

        return result;
    }

    // --- Mutating primitives ---

    public void ApplyLinearForce(Entity entity, double forceFraction)
    {
        if ((entity.Strength?.Value ?? 0.0) == 0.0 || forceFraction == 0.0)
        {
            return;
        }

        var facing = Direction.FromAngle(entity.FacingAngle);
        var perLegForce = entity.Strength!.Value * locomotionEfficiency;

        foreach (var leg in legs)
        {
            var (rx, ry) = leg.GetWorldOffset(entity.FacingAngle);
            entity.PendingImpulses.Add(new ForceVector(
                Fx: forceFraction * perLegForce * facing.X,
                Fy: forceFraction * perLegForce * facing.Y,
                Rx: rx,
                Ry: ry));
        }
    }

    public void ApplyTurnForce(Entity entity, double forceFraction)
    {
        if ((entity.Strength?.Value ?? 0.0) == 0.0 || forceFraction == 0.0)
        {
            return;
        }

        var facing = Direction.FromAngle(entity.FacingAngle);
        var perLegForce = entity.Strength!.Value * locomotionEfficiency;
        var scale = Math.Abs(forceFraction);
        var turnDirection = Math.Sign(forceFraction);

        foreach (var leg in legs)
        {
            var (rx, ry) = leg.GetWorldOffset(entity.FacingAngle);
            var goesForward = turnDirection > 0 ? leg.LateralOffset > 0 : leg.LateralOffset < 0;
            var legSign = goesForward ? 1.0 : -1.0;
            entity.PendingImpulses.Add(new ForceVector(
                Fx: legSign * perLegForce * scale * facing.X,
                Fy: legSign * perLegForce * scale * facing.Y,
                Rx: rx,
                Ry: ry));
        }
    }

    public void ApplyLegForce(Entity entity, string legName, double forceFraction)
    {
        if ((entity.Strength?.Value ?? 0.0) == 0.0 || forceFraction == 0.0)
        {
            return;
        }

        var leg = legs.FirstOrDefault(l => l.Name == legName);
        if (leg == null)
        {
            return;
        }

        var facing = Direction.FromAngle(entity.FacingAngle);
        var perLegForce = entity.Strength!.Value * locomotionEfficiency;
        var (rx, ry) = leg.GetWorldOffset(entity.FacingAngle);

        entity.PendingImpulses.Add(new ForceVector(
            Fx: forceFraction * perLegForce * facing.X,
            Fy: forceFraction * perLegForce * facing.Y,
            Rx: rx,
            Ry: ry));
    }

    public void ApplyLinearBrake(Entity entity, double forceFraction)
    {
        if ((entity.Strength?.Value ?? 0.0) == 0.0 || forceFraction == 0.0)
        {
            return;
        }

        if (entity.Velocity.Speed == 0.0)
        {
            return;
        }

        var facing = Direction.FromAngle(entity.FacingAngle);
        var perLegForce = entity.Strength!.Value * locomotionEfficiency * forceFraction;
        var totalBrakingForce = legs.Count * perLegForce;

        var brakingFx = -facing.X * totalBrakingForce;
        var brakingFy = -facing.Y * totalBrakingForce;
        var currentVelocity = new VelocityVector(entity.Velocity.Vx, entity.Velocity.Vy, 0.0);

        var (nextVx, nextVy) = PhysicsService.CalculateLinearVelocity(
            initialVelocity: currentVelocity,
            weight: entity.Weight,
            dragCoefficient: entity.DragCoefficient,
            bounds: entity.Bounds,
            fx: brakingFx,
            fy: brakingFy,
            groundFrictionCoefficient: entity.GroundFrictionCoefficient,
            isGrounded: entity.IsGrounded);

        var scale = 1.0;
        var dotProduct = nextVx * entity.Velocity.Vx + nextVy * entity.Velocity.Vy;
        if ((nextVx != 0.0 || nextVy != 0.0) && dotProduct < 0.0)
        {
            scale = BinarySearchLinearBrakeScale(
                currentVelocity: currentVelocity,
                brakingFx: brakingFx,
                brakingFy: brakingFy,
                originalVx: entity.Velocity.Vx,
                originalVy: entity.Velocity.Vy,
                weight: entity.Weight,
                dragCoefficient: entity.DragCoefficient,
                entity.Bounds,
                groundFrictionCoefficient: entity.GroundFrictionCoefficient,
                isGrounded: entity.IsGrounded);
        }

        foreach (var leg in legs)
        {
            var (rx, ry) = leg.GetWorldOffset(entity.FacingAngle);
            entity.PendingImpulses.Add(new ForceVector(
                Fx: -perLegForce * scale * facing.X,
                Fy: -perLegForce * scale * facing.Y,
                Rx: rx,
                Ry: ry));
        }
    }

    public void ApplyAngularBrake(Entity entity, double forceFraction)
    {
        if ((entity.Strength?.Value ?? 0.0) == 0.0 || forceFraction == 0.0)
        {
            return;
        }

        if (entity.Velocity.Omega == 0.0)
        {
            return;
        }

        var facing = Direction.FromAngle(entity.FacingAngle);
        var perLegForce = entity.Strength!.Value * locomotionEfficiency * forceFraction;

        var maxTurnForce = GetLegVectors(
            legs: legs,
            perLegForce: perLegForce,
            turnDirection: 1,
            entityAngle: entity.FacingAngle,
            scale: 1.0,
            facing: facing);
        var maxTurnTorque = PhysicsService.CalculateNetForceAndTorque(maxTurnForce).torque;

        if (maxTurnTorque == 0.0)
        {
            return;
        }

        var counterTorque = -Math.Sign(entity.Velocity.Omega) * maxTurnTorque;
        var currentVelocity = new VelocityVector(0.0, 0.0, entity.Velocity.Omega);
        var nextOmega = PhysicsService.CalculateAngularVelocity(
            initialVelocity: currentVelocity,
            weight: entity.Weight,
            height: entity.Height,
            width: entity.Width,
            angularDragCoefficient: entity.AngularDragCoefficient,
            torque: counterTorque,
            groundFrictionCoefficient: entity.GroundFrictionCoefficient,
            groundContactRadius: entity.GroundContactRadius,
            isGrounded: entity.IsGrounded);

        var scale = 1.0;
        if (nextOmega != 0.0 && Math.Sign(nextOmega) != Math.Sign(entity.Velocity.Omega))
        {
            scale = BinarySearchAngularBrakeScale(
                currentVelocity: currentVelocity,
                counterTorque: counterTorque,
                originalSign: Math.Sign(entity.Velocity.Omega),
                weight: entity.Weight,
                width: entity.Width,
                height: entity.Height,
                angularDragCoefficient: entity.AngularDragCoefficient,
                groundFrictionCoefficient: entity.GroundFrictionCoefficient,
                groundContactRadius: entity.GroundContactRadius,
                isGrounded: entity.IsGrounded);
        }

        var turnDirection = -Math.Sign(entity.Velocity.Omega);
        var legVectors = GetLegVectors(legs, perLegForce, turnDirection, entity.FacingAngle, scale, facing);
        entity.PendingImpulses.AddRange(legVectors);
    }

    // --- Private helpers ---

    private static IReadOnlyList<ForceVector> GetLegVectors(IReadOnlyList<Leg> legs, double perLegForce, int turnDirection, double entityAngle, double scale, Direction facing)
    {
        var vectors = new List<ForceVector>(legs.Count);
        foreach (var leg in legs)
        {
            var (rx, ry) = leg.GetWorldOffset(entityAngle);
            var goesForward = turnDirection >= 0 ? leg.LateralOffset > 0 : leg.LateralOffset < 0;
            var sign = goesForward ? 1.0 : -1.0;
            vectors.Add(new ForceVector(
                Fx: sign * perLegForce * scale * facing.X,
                Fy: sign * perLegForce * scale * facing.Y,
                Rx: rx,
                Ry: ry));
        }
        return vectors;
    }

    private static (long Ticks, double Distance) SimulateLinearStop(double vx, double vy, double brakingFx, double brakingFy, double weight, double dragCoefficient, Rectangle bounds, double groundFrictionCoefficient, bool isGrounded, int maxTicks)
    {
        var speed = Math.Sqrt(vx * vx + vy * vy);
        if (speed == 0.0)
        {
            return (0L, 0.0);
        }

        var totalDistance = 0.0;
        var ticks = 0L;
        var currentVelocity = new VelocityVector(vx, vy, 0.0);

        for (int n = 0; n < maxTicks; n++)
        {
            var (newVx, newVy) = PhysicsService.CalculateLinearVelocity(
                initialVelocity: currentVelocity,
                weight: weight,
                dragCoefficient: dragCoefficient,
                bounds: bounds,
                fx: brakingFx,
                fy: brakingFy,
                groundFrictionCoefficient: groundFrictionCoefficient,
                isGrounded: isGrounded);

            if (newVx == 0.0 && newVy == 0.0)
            {
                break;
            }

            var newSpeed = Math.Sqrt(newVx * newVx + newVy * newVy);
            var currentSpeed = Math.Sqrt(currentVelocity.Vx * currentVelocity.Vx + currentVelocity.Vy * currentVelocity.Vy);

            if (newSpeed >= currentSpeed)
            {
                break;
            }

            ticks++;
            totalDistance += newSpeed;
            currentVelocity = new VelocityVector(newVx, newVy, 0.0);
        }

        return (ticks, totalDistance);
    }

    private static (long Ticks, double Angle) SimulateAngularStop(double omega, double maxCounterTorque, double weight, double width, double height, double angularDragCoefficient, double groundFrictionCoefficient, double groundContactRadius, bool isGrounded, int maxTicks)
    {
        if (omega == 0.0)
        {
            return (0L, 0.0);
        }

        var counterTorque = -Math.Sign(omega) * maxCounterTorque;
        var originalSign = Math.Sign(omega);
        var totalAngle = 0.0;
        var ticks = 0L;
        var currentVelocity = new VelocityVector(0.0, 0.0, omega);

        for (int n = 0; n < maxTicks; n++)
        {
            var newOmega = PhysicsService.CalculateAngularVelocity(
                initialVelocity: currentVelocity,
                weight: weight,
                height: height,
                width: width,
                angularDragCoefficient: angularDragCoefficient,
                torque: counterTorque,
                groundFrictionCoefficient: groundFrictionCoefficient,
                groundContactRadius: groundContactRadius,
                isGrounded: isGrounded);

            if (newOmega == 0.0 || Math.Sign(newOmega) != originalSign)
            {
                break;
            }

            ticks++;
            totalAngle += Math.Abs(newOmega);
            currentVelocity = new VelocityVector(0.0, 0.0, newOmega);
        }

        return (ticks, totalAngle);
    }

    private static double BinarySearchLinearBrakeScale(VelocityVector currentVelocity, double brakingFx, double brakingFy, double originalVx, double originalVy, double weight, double dragCoefficient, Rectangle bounds, double groundFrictionCoefficient, bool isGrounded)
    {
        var lo = 0.0;
        var hi = 1.0;

        for (var i = 0; i < BrakeSearchSteps; i++)
        {
            var mid = (lo + hi) / 2.0;
            var (newVx, newVy) = PhysicsService.CalculateLinearVelocity(
                initialVelocity: currentVelocity,
                weight: weight,
                dragCoefficient: dragCoefficient,
                bounds: bounds,
                fx: mid * brakingFx,
                fy: mid * brakingFy,
                groundFrictionCoefficient: groundFrictionCoefficient,
                isGrounded: isGrounded);

            var dotProduct = newVx * originalVx + newVy * originalVy;
            if ((newVx == 0.0 && newVy == 0.0) || dotProduct >= 0.0)
            {
                lo = mid;
            }
            else
            {
                hi = mid;
            }
        }

        return lo;
    }

    private static double BinarySearchAngularBrakeScale(VelocityVector currentVelocity, double counterTorque, int originalSign, double weight, double width, double height, double angularDragCoefficient, double groundFrictionCoefficient, double groundContactRadius, bool isGrounded)
    {
        var lo = 0.0;
        var hi = 1.0;

        for (var i = 0; i < BrakeSearchSteps; i++)
        {
            var mid = (lo + hi) / 2.0;
            var newOmega = PhysicsService.CalculateAngularVelocity(
                initialVelocity: currentVelocity,
                weight: weight,
                height: height,
                width: width,
                angularDragCoefficient: angularDragCoefficient,
                torque: mid * counterTorque,
                groundFrictionCoefficient: groundFrictionCoefficient,
                groundContactRadius: groundContactRadius,
                isGrounded: isGrounded);

            if (newOmega == 0.0 || Math.Sign(newOmega) == originalSign)
            {
                lo = mid;
            }
            else
            {
                hi = mid;
            }
        }

        return lo;
    }
}
