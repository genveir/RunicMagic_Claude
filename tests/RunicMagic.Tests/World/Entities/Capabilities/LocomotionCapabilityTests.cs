using RunicMagic.World;
using RunicMagic.World.Entities;
using RunicMagic.World.Entities.Capabilities;
using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Motion.Simulated;

namespace RunicMagic.Tests.World.Entities.Capabilities;

public class LocomotionCapabilityTests
{
    private static readonly Leg LeftLeg = new Leg(lateralOffset: -100, forwardOffset: 0);
    private static readonly Leg RightLeg = new Leg(lateralOffset: 100, forwardOffset: 0);
    private static readonly IWorldEventTracker Tracker = new NullTracker();

    private static LocomotionCapability MakeLocomotion(double efficiency = 1.0)
    {
        var legs = new List<Leg> { LeftLeg, RightLeg };
        return new LocomotionCapability(legs, efficiency);
    }

    private static Entity MakeEntity(
        long x = 0, long y = 0,
        long strength = 1000,
        long weight = 1000,
        long width = 200, long height = 200,
        double angle = 0,
        double dragCoefficient = 0,
        double angularDragCoefficient = 0)
    {
        return new EntityBuilder()
            .WithLocation(x, y)
            .WithStrength(strength)
            .WithWeight(weight)
            .WithSize(width, height)
            .WithAngle(angle)
            .WithDragCoefficient(dragCoefficient)
            .WithAngularDragCoefficient(angularDragCoefficient)
            .Build();
    }

    private sealed class NullTracker : IWorldEventTracker
    {
        public void Add(WorldEvent @event) { }
        public void Track(Entity entity) { }
    }

    // --- LocomotionEfficiency ---

    [Fact]
    public void LocomotionEfficiency_IsStoredCorrectly()
    {
        var legs = new List<Leg> { RightLeg };
        var locomotion = new LocomotionCapability(legs, locomotionEfficiency: 0.75);

        locomotion.LocomotionEfficiency.Should().Be(0.75);
    }

    // --- GetLegVectors ---

    [Fact]
    public void GetLegVectors_TurnDirectionPositive_PositiveLateralLegGoesForward()
    {
        var legs = new List<Leg> { LeftLeg, RightLeg };
        var result = LocomotionCapability.GetLegVectors(
            legs: legs,
            perLegForce: 100,
            turnDirection: 1,
            entityAngle: 0,
            scale: 1,
            cosA: 1,
            sinA: 0);

        // LateralOffset < 0 (left) → backward; LateralOffset > 0 (right) → forward
        result[0].Fx.Should().BeApproximately(-100, 0.001);
        result[1].Fx.Should().BeApproximately(100, 0.001);
    }

    [Fact]
    public void GetLegVectors_TurnDirectionNegative_NegativeLateralLegGoesForward()
    {
        var legs = new List<Leg> { LeftLeg, RightLeg };
        var result = LocomotionCapability.GetLegVectors(
            legs: legs,
            perLegForce: 100,
            turnDirection: -1,
            entityAngle: 0,
            scale: 1,
            cosA: 1,
            sinA: 0);

        // LateralOffset < 0 (left) → forward; LateralOffset > 0 (right) → backward
        result[0].Fx.Should().BeApproximately(100, 0.001);
        result[1].Fx.Should().BeApproximately(-100, 0.001);
    }

    [Fact]
    public void GetLegVectors_ScaleHalf_ForceMagnitudeHalved()
    {
        var legs = new List<Leg> { RightLeg };
        var full = LocomotionCapability.GetLegVectors(
            legs: legs, perLegForce: 100, turnDirection: 1, entityAngle: 0, scale: 1, cosA: 1, sinA: 0);
        var half = LocomotionCapability.GetLegVectors(
            legs: legs, perLegForce: 100, turnDirection: 1, entityAngle: 0, scale: 0.5, cosA: 1, sinA: 0);

        half[0].Fx.Should().BeApproximately(full[0].Fx / 2, 0.001);
    }

    [Fact]
    public void GetLegVectors_LegWorldOffset_RotatesWithEntityAngle()
    {
        var legs = new List<Leg> { RightLeg };
        var angle = Math.PI / 2;
        var cosA = Math.Cos(angle);
        var sinA = Math.Sin(angle);

        var result = LocomotionCapability.GetLegVectors(
            legs: legs, perLegForce: 100, turnDirection: 1, entityAngle: angle, scale: 1, cosA: cosA, sinA: sinA);

        // RightLeg at 90°: rx = 100*sin(90°) = 100, ry = -100*cos(90°) = 0
        result[0].Rx.Should().BeApproximately(100, 0.001);
        result[0].Ry.Should().BeApproximately(0, 0.001);
    }

    // --- SimulateAngularBrakingAngle ---

    [Fact]
    public void SimulateAngularBrakingAngle_ZeroOmega_ReturnsZero()
    {
        var result = LocomotionCapability.SimulateAngularBrakingAngle(
            omega: 0,
            maxCounterTorque: 1000,
            weight: 1000,
            width: 200,
            height: 200,
            angularDragCoefficient: 0);

        result.Should().Be(0.0);
    }

    [Fact]
    public void SimulateAngularBrakingAngle_PositiveOmega_ReturnsPositiveValue()
    {
        var result = LocomotionCapability.SimulateAngularBrakingAngle(
            omega: 0.1,
            maxCounterTorque: 100,
            weight: 1000,
            width: 200,
            height: 200,
            angularDragCoefficient: 0);

        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public void SimulateAngularBrakingAngle_NegativeOmega_ReturnsPositiveValue()
    {
        var result = LocomotionCapability.SimulateAngularBrakingAngle(
            omega: -0.1,
            maxCounterTorque: 100,
            weight: 1000,
            width: 200,
            height: 200,
            angularDragCoefficient: 0);

        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public void SimulateAngularBrakingAngle_SymmetricPositiveAndNegativeOmega()
    {
        var positiveResult = LocomotionCapability.SimulateAngularBrakingAngle(
            omega: 0.1,
            maxCounterTorque: 100,
            weight: 1000,
            width: 200,
            height: 200,
            angularDragCoefficient: 0);

        var negativeResult = LocomotionCapability.SimulateAngularBrakingAngle(
            omega: -0.1,
            maxCounterTorque: 100,
            weight: 1000,
            width: 200,
            height: 200,
            angularDragCoefficient: 0);

        negativeResult.Should().BeApproximately(positiveResult, 0.001);
    }

    [Fact]
    public void SimulateAngularBrakingAngle_LargerOmega_RequiresMoreAngle()
    {
        var smallResult = LocomotionCapability.SimulateAngularBrakingAngle(
            omega: 0.05,
            maxCounterTorque: 100,
            weight: 1000,
            width: 200,
            height: 200,
            angularDragCoefficient: 0);

        var largeResult = LocomotionCapability.SimulateAngularBrakingAngle(
            omega: 0.1,
            maxCounterTorque: 100,
            weight: 1000,
            width: 200,
            height: 200,
            angularDragCoefficient: 0);

        largeResult.Should().BeGreaterThan(smallResult);
    }

    // --- SimulateLinearBrakingDistance ---

    [Fact]
    public void SimulateLinearBrakingDistance_ZeroVelocity_ReturnsZero()
    {
        var bounds = new Rectangle(new Location(0, 0), Width: 200, Height: 200, Angle: 0);
        var result = LocomotionCapability.SimulateLinearBrakingDistance(
            vx: 0, vy: 0,
            brakingFx: -100, brakingFy: 0,
            weight: 1000,
            dragCoefficient: 0,
            bounds: bounds);

        result.Should().Be(0.0);
    }

    [Fact]
    public void SimulateLinearBrakingDistance_NonZeroVelocity_ReturnsPositiveValue()
    {
        var bounds = new Rectangle(new Location(0, 0), Width: 200, Height: 200, Angle: 0);
        var result = LocomotionCapability.SimulateLinearBrakingDistance(
            vx: 10, vy: 0,
            brakingFx: -1, brakingFy: 0,
            weight: 1000,
            dragCoefficient: 0,
            bounds: bounds);

        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public void SimulateLinearBrakingDistance_LargerVelocity_RequiresMoreDistance()
    {
        var bounds = new Rectangle(new Location(0, 0), Width: 200, Height: 200, Angle: 0);
        var smallResult = LocomotionCapability.SimulateLinearBrakingDistance(
            vx: 5, vy: 0,
            brakingFx: -1, brakingFy: 0,
            weight: 1000,
            dragCoefficient: 0,
            bounds: bounds);

        var largeResult = LocomotionCapability.SimulateLinearBrakingDistance(
            vx: 10, vy: 0,
            brakingFx: -1, brakingFy: 0,
            weight: 1000,
            dragCoefficient: 0,
            bounds: bounds);

        largeResult.Should().BeGreaterThan(smallResult);
    }

    // --- BinarySearchBrakeScale ---

    [Fact]
    public void BinarySearchBrakeScale_ResultIsBetweenZeroAndOne()
    {
        var velocity = new VelocityVector(0, 0, Omega: 0.5);
        var result = LocomotionCapability.BinarySearchBrakeScale(
            currentVelocity: velocity,
            counterTorque: -100000,
            originalSign: 1,
            weight: 1000,
            width: 200,
            height: 200,
            angularDragCoefficient: 0);

        result.Should().BeGreaterThanOrEqualTo(0.0);
        result.Should().BeLessThanOrEqualTo(1.0);
    }

    [Fact]
    public void BinarySearchBrakeScale_AtReturnedScale_OmegaDoesNotOvershoottZero()
    {
        var velocity = new VelocityVector(0, 0, Omega: 0.5);
        const double counterTorque = -100000;
        const double weight = 1000;
        const double width = 200;
        const double height = 200;

        var scale = LocomotionCapability.BinarySearchBrakeScale(
            currentVelocity: velocity,
            counterTorque: counterTorque,
            originalSign: 1,
            weight: weight,
            width: width,
            height: height,
            angularDragCoefficient: 0);

        var newOmega = PhysicsService.CalculateAngularVelocity(
            initialVelocity: velocity,
            weight: weight,
            height: height,
            width: width,
            angularDragCoefficient: 0,
            torque: scale * counterTorque);

        // The returned scale must not flip the sign of omega (no overshoot past zero)
        (newOmega == 0.0 || Math.Sign(newOmega) == 1).Should().BeTrue();
    }

    // --- ApplyTurning ---

    [Fact]
    public void ApplyTurning_AddsImpulses_WhenAngularErrorIsSignificant()
    {
        var locomotion = MakeLocomotion();
        var entity = MakeEntity();

        locomotion.ApplyTurning(
            entity: entity,
            angularError: 1.0,
            perLegForce: 100,
            cosA: 1,
            sinA: 0,
            tracker: Tracker);

        entity.PendingImpulses.Should().HaveCount(2);
    }

    [Fact]
    public void ApplyTurning_DoesNotAddImpulses_WhenLegsProduceNoTorque()
    {
        // Legs on the body centerline produce no torque; the capability has nothing to work with
        var legs = new List<Leg> { new Leg(lateralOffset: 0, forwardOffset: 0) };
        var locomotion = new LocomotionCapability(legs, locomotionEfficiency: 1.0);
        var entity = MakeEntity();

        locomotion.ApplyTurning(
            entity: entity,
            angularError: 1.0,
            perLegForce: 100,
            cosA: 1,
            sinA: 0,
            tracker: Tracker);

        entity.PendingImpulses.Should().BeEmpty();
    }

    [Fact]
    public void ApplyTurning_Coasts_WhenCurrentOmegaAloneReachesTarget()
    {
        // Oversized legs on a near-weightless entity: braking is instantaneous (brakingAngle ≈ 0).
        // omega (0.1) already exceeds the angular error (0.05), so no further impulse is needed.
        var legs = new List<Leg>
        {
            new Leg(lateralOffset: -500, forwardOffset: 0),
            new Leg(lateralOffset: 500, forwardOffset: 0),
        };
        var locomotion = new LocomotionCapability(legs, locomotionEfficiency: 1.0);
        var entity = new EntityBuilder()
            .WithStrength(100000)
            .WithWeight(1)
            .WithSize(200, 200)
            .Build();
        entity.Velocity = new VelocityVector(0, 0, Omega: 0.1);

        locomotion.ApplyTurning(
            entity: entity,
            angularError: 0.05,
            perLegForce: 100000,
            cosA: 1,
            sinA: 0,
            tracker: Tracker);

        entity.PendingImpulses.Should().BeEmpty();
    }

    [Fact]
    public void ApplyTurning_AddsBrakingImpulses_WhenOmegaWouldOvershoottTarget()
    {
        // Large omega, tiny angular error: the entity would overshoot without braking.
        var locomotion = MakeLocomotion();
        var entity = MakeEntity(strength: 1000, weight: 1000, width: 200, height: 200);
        entity.Velocity = new VelocityVector(0, 0, Omega: 2.0);

        locomotion.ApplyTurning(
            entity: entity,
            angularError: 0.01,
            perLegForce: 1000,
            cosA: 1,
            sinA: 0,
            tracker: Tracker);

        entity.PendingImpulses.Should().NotBeEmpty();
    }

    // --- ApplyWalking ---

    [Fact]
    public void ApplyWalking_NoImpulses_WhenAtDestinationAndStopped()
    {
        var locomotion = MakeLocomotion();
        var entity = MakeEntity();

        locomotion.ApplyWalking(
            entity: entity,
            desiredDestination: new Location(0, 0),
            perLegForce: 100,
            cosA: 1,
            sinA: 0,
            tracker: Tracker);

        entity.PendingImpulses.Should().BeEmpty();
    }

    [Fact]
    public void ApplyWalking_AddsBrakingImpulses_WhenAtDestinationButMoving()
    {
        var locomotion = MakeLocomotion();
        var entity = MakeEntity();
        entity.Velocity = new VelocityVector(Vx: 10, Vy: 0, Omega: 0);

        locomotion.ApplyWalking(
            entity: entity,
            desiredDestination: new Location(0, 0),
            perLegForce: 100,
            cosA: 1,
            sinA: 0,
            tracker: Tracker);

        entity.PendingImpulses.Should().NotBeEmpty();
        entity.PendingImpulses.Sum(v => v.Fx).Should().BeLessThan(0);
    }

    [Fact]
    public void ApplyWalking_AddsForwardImpulses_WhenFarFromDestination()
    {
        var locomotion = MakeLocomotion();
        var entity = MakeEntity();

        locomotion.ApplyWalking(
            entity: entity,
            desiredDestination: new Location(10000, 0),
            perLegForce: 100,
            cosA: 1,
            sinA: 0,
            tracker: Tracker);

        entity.PendingImpulses.Should().HaveCount(2);
        entity.PendingImpulses.Sum(v => v.Fx).Should().BeGreaterThan(0);
    }

    [Fact]
    public void ApplyWalking_AddsBrakingImpulses_WhenTooFastToStop()
    {
        // Entity moving at 5000 mm/tick with perLegForce=100; braking distance >> 200 mm
        var locomotion = MakeLocomotion();
        var entity = MakeEntity();
        entity.Velocity = new VelocityVector(Vx: 5000, Vy: 0, Omega: 0);

        locomotion.ApplyWalking(
            entity: entity,
            desiredDestination: new Location(200, 0),
            perLegForce: 100,
            cosA: 1,
            sinA: 0,
            tracker: Tracker);

        entity.PendingImpulses.Should().NotBeEmpty();
        entity.PendingImpulses.Sum(v => v.Fx).Should().BeLessThan(0);
    }

    [Fact]
    public void ApplyWalking_NoImpulses_WhenCoastingToDestination()
    {
        // Near-weightless entity: braking is instantaneous (brakingDistance = 0).
        // Speed (150 mm/tick) equals remaining distance (150 mm) → coast condition fires.
        var locomotion = MakeLocomotion();
        var entity = new EntityBuilder()
            .WithLocation(0, 0)
            .WithWeight(1)
            .WithSize(200, 200)
            .Build();
        entity.Velocity = new VelocityVector(Vx: 150, Vy: 0, Omega: 0);

        locomotion.ApplyWalking(
            entity: entity,
            desiredDestination: new Location(150, 0),
            perLegForce: 5000,
            cosA: 1,
            sinA: 0,
            tracker: Tracker);

        entity.PendingImpulses.Should().BeEmpty();
    }

    // --- Walk and Run ---

    [Fact]
    public void Walk_ProducesSeventyPercentForce_ComparedToRun()
    {
        var locomotion = MakeLocomotion();
        var walkEntity = MakeEntity(strength: 1000);
        var runEntity = MakeEntity(strength: 1000);
        var destination = new Location(10000, 0);

        locomotion.Walk(walkEntity, destination, Tracker);
        locomotion.Run(runEntity, destination, Tracker);

        var walkFx = walkEntity.PendingImpulses.Sum(v => v.Fx);
        var runFx = runEntity.PendingImpulses.Sum(v => v.Fx);

        (walkFx / runFx).Should().BeApproximately(0.7, 0.001);
    }

    // --- ApplyLocomotion (dispatch) ---

    [Fact]
    public void ApplyLocomotion_WhenAlignedAndFarAway_AddsForwardImpulses()
    {
        var locomotion = MakeLocomotion();
        var entity = MakeEntity(strength: 1000);

        locomotion.ApplyLocomotion(entity, new Location(10000, 0), forceFraction: 1.0, tracker: Tracker);

        entity.PendingImpulses.Should().NotBeEmpty();
        entity.PendingImpulses.Sum(v => v.Fx).Should().BeGreaterThan(0);
    }

    [Fact]
    public void ApplyLocomotion_WhenMisaligned_AddsTurningImpulses()
    {
        var locomotion = MakeLocomotion();
        var entity = MakeEntity(strength: 1000); // facing right (angle = 0)

        // 90° off to the side: forces turning, not walking
        locomotion.ApplyLocomotion(entity, new Location(0, 10000), forceFraction: 1.0, tracker: Tracker);

        entity.PendingImpulses.Should().NotBeEmpty();
    }

    [Fact]
    public void ApplyLocomotion_WithinArrivalThresholdAndStopped_AddsNoImpulses()
    {
        var locomotion = MakeLocomotion();
        var entity = MakeEntity();

        // 50 mm away is below the 100 mm arrival threshold; entity is already stopped
        locomotion.ApplyLocomotion(entity, new Location(50, 0), forceFraction: 1.0, tracker: Tracker);

        entity.PendingImpulses.Should().BeEmpty();
    }
}
