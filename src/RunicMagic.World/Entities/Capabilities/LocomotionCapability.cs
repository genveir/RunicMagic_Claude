using RunicMagic.World.Geometry;
using RunicMagic.World.Motion.Simulated;

namespace RunicMagic.World.Entities.Capabilities;

public class LocomotionCapability
{
    private const double WalkForceFraction = 0.7;
    private const double StraightThresholdRad = 0.05;
    private const double ArrivalThresholdMm = 100.0;
    private const int BrakeSearchSteps = 4;

    private readonly IReadOnlyList<Leg> _legs;

    public LocomotionCapability(IReadOnlyList<Leg> legs, double locomotionEfficiency)
    {
        _legs = legs;
        LocomotionEfficiency = locomotionEfficiency;
    }

    public double LocomotionEfficiency { get; }

    public void Walk(Entity entity, Location desiredDestination, IWorldEventTracker tracker)
    {
        ApplyLocomotion(entity, desiredDestination, WalkForceFraction, tracker);
    }

    public void Run(Entity entity, Location desiredDestination, IWorldEventTracker tracker)
    {
        ApplyLocomotion(entity, desiredDestination, 1.0, tracker);
    }

    internal void ApplyLocomotion(Entity entity, Location desiredDestination, double forceFraction, IWorldEventTracker tracker)
    {
        var cosA = Math.Cos(entity.Angle);
        var sinA = Math.Sin(entity.Angle);
        var perLegForce = (double)entity.Strength * LocomotionEfficiency * forceFraction;

        var dx = desiredDestination.X - entity.Location.X;
        var dy = desiredDestination.Y - entity.Location.Y;
        var remainingDistance = Math.Sqrt(dx * dx + dy * dy);

        if (remainingDistance < ArrivalThresholdMm)
        {
            ApplyWalking(entity, desiredDestination, perLegForce, cosA, sinA, tracker);
            return;
        }

        var desiredDirection = Direction.FromPoints(entity.Location, desiredDestination);
        var cross = cosA * desiredDirection.Y - sinA * desiredDirection.X;
        var dot = cosA * desiredDirection.X + sinA * desiredDirection.Y;
        var angularError = Math.Atan2(cross, dot);

        if (Math.Abs(angularError) >= StraightThresholdRad || (entity.Velocity?.Omega ?? 0.0) != 0.0)
        {
            ApplyTurning(entity, angularError, perLegForce, cosA, sinA, tracker);
            return;
        }

        ApplyWalking(entity, desiredDestination, perLegForce, cosA, sinA, tracker);
    }

    internal void ApplyTurning(Entity entity, double angularError, double perLegForce, double cosA, double sinA, IWorldEventTracker tracker)
    {
        var maxTurnForce = GetLegVectors(
            legs: _legs,
            perLegForce: perLegForce,
            turnDirection: 1,
            entityAngle: entity.Angle,
            scale: 1,
            cosA: cosA,
            sinA: sinA);

        var maxTurnTorque = PhysicsService.CalculateNetForceAndTorque(maxTurnForce).torque;

        if (maxTurnTorque == 0.0)
        {
            return;
        }

        var omega = entity.Velocity?.Omega ?? 0.0;
        var brakingAngle = SimulateAngularBrakingAngle(
            omega: omega,
            maxCounterTorque: maxTurnTorque,
            weight: entity.Weight,
            width: entity.Width,
            height: entity.Height,
            angularDragCoefficient: entity.AngularDragCoefficient,
            groundFrictionCoefficient: entity.GroundFrictionCoefficient,
            groundContactRadius: entity.GroundContactRadius,
            isGrounded: entity.IsGrounded);

        var shouldBrake = brakingAngle >= Math.Abs(angularError);
        var shouldCoast = !shouldBrake && Math.Abs(omega) + brakingAngle >= Math.Abs(angularError);

        if (shouldCoast)
        {
            return;
        }

        var turnDirection = shouldBrake ? -Math.Sign(omega) : Math.Sign(angularError);
        var scale = 1.0;

        if (shouldBrake)
        {
            var counterTorque = -Math.Sign(omega) * maxTurnTorque;
            var currentVelocity = new VelocityVector(0.0, 0.0, omega);
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

            if (nextOmega != 0.0 && Math.Sign(nextOmega) != Math.Sign(omega))
            {
                scale = BinarySearchBrakeScale(
                    currentVelocity: currentVelocity,
                    counterTorque: counterTorque,
                    originalSign: Math.Sign(omega),
                    weight: entity.Weight,
                    width: entity.Width,
                    height: entity.Height,
                    angularDragCoefficient: entity.AngularDragCoefficient,
                    groundFrictionCoefficient: entity.GroundFrictionCoefficient,
                    groundContactRadius: entity.GroundContactRadius,
                    isGrounded: entity.IsGrounded);
            }
        }

        var legVectors = GetLegVectors(_legs, perLegForce, turnDirection, entity.Angle, scale, cosA, sinA);

        entity.PendingImpulses.AddRange(legVectors);
    }

    internal static IReadOnlyList<ForceVector> GetLegVectors(IReadOnlyList<Leg> legs, double perLegForce, int turnDirection, double entityAngle, double scale, double cosA, double sinA)
    {
        var vectors = new List<ForceVector>(legs.Count);
        foreach (var leg in legs)
        {
            var (rx, ry) = leg.GetWorldOffset(entityAngle);
            var goesForward = turnDirection >= 0 ? leg.LateralOffset > 0 : leg.LateralOffset < 0;
            var sign = goesForward ? 1.0 : -1.0;
            vectors.Add(new ForceVector(
                Fx: sign * perLegForce * scale * cosA,
                Fy: sign * perLegForce * scale * sinA,
                Rx: rx,
                Ry: ry));
        }
        return vectors;
    }

    internal static double BinarySearchBrakeScale(VelocityVector currentVelocity, double counterTorque, int originalSign, double weight, double width, double height, double angularDragCoefficient, double groundFrictionCoefficient, double groundContactRadius, bool isGrounded)
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

    internal void ApplyWalking(Entity entity, Location desiredDestination, double perLegForce, double cosA, double sinA, IWorldEventTracker tracker)
    {
        var dx = desiredDestination.X - entity.Location.X;
        var dy = desiredDestination.Y - entity.Location.Y;
        var remainingDistance = Math.Sqrt(dx * dx + dy * dy);

        var vx = entity.Velocity?.Vx ?? 0.0;
        var vy = entity.Velocity?.Vy ?? 0.0;
        var speed = Math.Sqrt(vx * vx + vy * vy);

        bool shouldStop = false;
        if (remainingDistance < ArrivalThresholdMm)
        {
            if (speed == 0.0)
            {
                return;
            }

            shouldStop = true;
        }

        var totalBrakingForce = _legs.Count * perLegForce;
        var bounds = new Rectangle(entity.Location, Width: entity.Width, Height: entity.Height, Angle: entity.Angle);
        var brakingFx = -cosA * totalBrakingForce;
        var brakingFy = -sinA * totalBrakingForce;

        var brakingDistance = SimulateLinearBrakingDistance(
            vx: vx,
            vy: vy,
            brakingFx: brakingFx,
            brakingFy: brakingFy,
            weight: entity.Weight,
            dragCoefficient: entity.DragCoefficient,
            bounds: bounds,
            groundFrictionCoefficient: entity.GroundFrictionCoefficient,
            isGrounded: entity.IsGrounded);

        var shouldBrake = brakingDistance >= remainingDistance || shouldStop;
        var shouldCoast = !shouldBrake && speed + brakingDistance >= remainingDistance;

        if (shouldCoast)
        {
            return;
        }

        var forwardSign = shouldBrake ? -1.0 : 1.0;
        var scale = 1.0;

        if (shouldBrake)
        {
            var currentVelocity = new VelocityVector(vx, vy, 0.0);
            var (nextVx, nextVy) = PhysicsService.CalculateLinearVelocity(
                initialVelocity: currentVelocity,
                weight: entity.Weight,
                dragCoefficient: entity.DragCoefficient,
                bounds: bounds,
                fx: brakingFx,
                fy: brakingFy,
                groundFrictionCoefficient: entity.GroundFrictionCoefficient,
                isGrounded: entity.IsGrounded);

            var dotProduct = nextVx * vx + nextVy * vy;
            if ((nextVx != 0.0 || nextVy != 0.0) && dotProduct < 0.0)
            {
                scale = BinarySearchLinearBrakeScale(
                    currentVelocity: currentVelocity,
                    brakingFx: brakingFx,
                    brakingFy: brakingFy,
                    originalVx: vx,
                    originalVy: vy,
                    weight: entity.Weight,
                    dragCoefficient: entity.DragCoefficient,
                    bounds: bounds,
                    groundFrictionCoefficient: entity.GroundFrictionCoefficient,
                    isGrounded: entity.IsGrounded);
            }
        }

        foreach (var leg in _legs)
        {
            var (rx, ry) = leg.GetWorldOffset(entity.Angle);
            entity.PendingImpulses.Add(new ForceVector(
                Fx: forwardSign * perLegForce * scale * cosA,
                Fy: forwardSign * perLegForce * scale * sinA,
                Rx: rx,
                Ry: ry));
        }
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

    internal static double SimulateAngularBrakingAngle(double omega, double maxCounterTorque, double weight, double width, double height, double angularDragCoefficient, double groundFrictionCoefficient, double groundContactRadius, bool isGrounded)
    {
        if (omega == 0.0)
        {
            return 0.0;
        }

        var counterTorque = -Math.Sign(omega) * maxCounterTorque;
        var originalSign = Math.Sign(omega);
        var totalAngle = 0.0;
        var currentVelocity = new VelocityVector(0.0, 0.0, omega);

        while (true)
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

            if (newOmega == 0.0)
            {
                break;
            }

            if (Math.Sign(newOmega) != originalSign)
            {
                break;
            }

            totalAngle += Math.Abs(newOmega);
            currentVelocity = new VelocityVector(0.0, 0.0, newOmega);
        }

        return totalAngle;
    }

    internal static double SimulateLinearBrakingDistance(double vx, double vy, double brakingFx, double brakingFy, double weight, double dragCoefficient, Rectangle bounds, double groundFrictionCoefficient, bool isGrounded)
    {
        var speed = Math.Sqrt(vx * vx + vy * vy);
        if (speed == 0.0)
        {
            return 0.0;
        }

        var totalDistance = 0.0;
        var currentVelocity = new VelocityVector(vx, vy, 0.0);

        while (true)
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

            totalDistance += newSpeed;
            currentVelocity = new VelocityVector(newVx, newVy, 0.0);
        }

        return totalDistance;
    }
}
