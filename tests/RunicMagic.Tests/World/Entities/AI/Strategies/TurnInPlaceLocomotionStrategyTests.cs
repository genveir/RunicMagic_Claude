using RunicMagic.World.Entities;
using RunicMagic.World.Entities.AI.Strategies;
using RunicMagic.World.Geometry;
using RunicMagic.World.Motion.Simulated;

namespace RunicMagic.Tests.World.Entities.AI.Strategies;

public class TurnInPlaceLocomotionStrategyTests
{
    private const double OrientedThresholdRad = 0.05;

    private static Entity MakeEntity(double angle = 0, long strength = 1000, long weight = 1000)
    {
        return new EntityBuilder()
            .WithLocation(0, 0)
            .WithStrength(strength)
            .WithWeight(weight)
            .WithSize(200, 200)
            .WithAngle(angle)
            .WithLocomotion(efficiency: 1.0)
            .Build();
    }

    // --- No locomotion ---

    [Fact]
    public void Execute_WithNoLocomotion_ReturnsCannotMove()
    {
        var entity = new EntityBuilder()
            .WithLocation(0, 0)
            .WithSize(200, 200)
            .Build();

        var result = TurnInPlaceLocomotionStrategy.Execute(entity, Direction.FromAngle(0), forceFraction: 1.0, orientedThresholdRad: OrientedThresholdRad);

        result.Should().Be(TurnInPlaceResult.CannotMove);
    }

    // --- Already oriented ---

    [Fact]
    public void Execute_WhenAlreadyOriented_AndStopped_ReturnsOriented()
    {
        var entity = MakeEntity(angle: 0);

        var result = TurnInPlaceLocomotionStrategy.Execute(entity, Direction.FromAngle(0), forceFraction: 1.0, orientedThresholdRad: OrientedThresholdRad);

        result.Should().Be(TurnInPlaceResult.Oriented);
    }

    [Fact]
    public void Execute_WhenAlreadyOriented_AndStopped_AddsNoImpulses()
    {
        var entity = MakeEntity(angle: 0);

        TurnInPlaceLocomotionStrategy.Execute(entity, Direction.FromAngle(0), forceFraction: 1.0, orientedThresholdRad: OrientedThresholdRad);

        entity.PendingImpulses.Should().BeEmpty();
    }

    [Fact]
    public void Execute_WhenWithinThreshold_ButStillSpinning_ReturnsNotOriented()
    {
        var entity = MakeEntity(angle: 0);
        entity.Velocity = new VelocityVector(Vx: 0, Vy: 0, Omega: 0.01);

        var result = TurnInPlaceLocomotionStrategy.Execute(entity, Direction.FromAngle(0), forceFraction: 1.0, orientedThresholdRad: OrientedThresholdRad);

        result.Should().Be(TurnInPlaceResult.NotOriented);
    }

    // --- Turning ---

    [Fact]
    public void Execute_WhenMisaligned_ReturnsNotOriented()
    {
        var entity = MakeEntity(angle: 0);

        var result = TurnInPlaceLocomotionStrategy.Execute(entity, Direction.FromAngle(Math.PI / 2), forceFraction: 1.0, orientedThresholdRad: OrientedThresholdRad);

        result.Should().Be(TurnInPlaceResult.NotOriented);
    }

    [Fact]
    public void Execute_WhenMisaligned_AddsTurningImpulses()
    {
        var entity = MakeEntity(angle: 0);

        TurnInPlaceLocomotionStrategy.Execute(entity, Direction.FromAngle(Math.PI / 2), forceFraction: 1.0, orientedThresholdRad: OrientedThresholdRad);

        entity.PendingImpulses.Should().NotBeEmpty();
    }

    [Fact]
    public void Execute_TurnsTowardTarget_PositiveError_AddsPositiveTorque()
    {
        var entity = MakeEntity(angle: 0); // facing right

        // Target is to the left (positive angular error)
        TurnInPlaceLocomotionStrategy.Execute(entity, Direction.FromAngle(Math.PI / 2), forceFraction: 1.0, orientedThresholdRad: OrientedThresholdRad);

        // Net torque = sum of (Rx * Fy - Ry * Fx) for each impulse
        entity.PendingImpulses.Sum(v => v.Rx * v.Fy - v.Ry * v.Fx).Should().BeGreaterThan(0);
    }

    [Fact]
    public void Execute_TurnsTowardTarget_NegativeError_AddsNegativeTorque()
    {
        var entity = MakeEntity(angle: 0); // facing right

        // Target is to the right/below (negative angular error)
        TurnInPlaceLocomotionStrategy.Execute(entity, Direction.FromAngle(-Math.PI / 2), forceFraction: 1.0, orientedThresholdRad: OrientedThresholdRad);

        // Net torque = sum of (Rx * Fy - Ry * Fx) for each impulse
        entity.PendingImpulses.Sum(v => v.Rx * v.Fy - v.Ry * v.Fx).Should().BeLessThan(0);
    }

    // --- Coasting ---

    [Fact]
    public void Execute_CoastsWhenOmegaAloneReachesTarget_AddsNoImpulses()
    {
        // Near-weightless entity: braking is instantaneous (brakingAngle ≈ 0).
        // omega (0.1) already exceeds the angular error (0.05), so no further impulse is needed.
        var entity = MakeEntity(strength: 100000, weight: 1);

        entity.Velocity = new VelocityVector(Vx: 0, Vy: 0, Omega: 0.1);

        TurnInPlaceLocomotionStrategy.Execute(entity, Direction.FromAngle(0.05), forceFraction: 1.0, orientedThresholdRad: OrientedThresholdRad);

        entity.PendingImpulses.Should().BeEmpty();
    }

    // --- Braking ---

    [Fact]
    public void Execute_AddsBrakingImpulses_WhenOmegaWouldOvershootTarget()
    {
        var entity = MakeEntity(strength: 1000, weight: 1000);
        entity.Velocity = new VelocityVector(Vx: 0, Vy: 0, Omega: 2.0);

        // Tiny angular error: entity would overshoot without braking
        TurnInPlaceLocomotionStrategy.Execute(entity, Direction.FromAngle(0.01), forceFraction: 1.0, orientedThresholdRad: OrientedThresholdRad);

        entity.PendingImpulses.Should().NotBeEmpty();
    }

    // --- Force fraction ---

    [Fact]
    public void Execute_HigherForceFraction_ProducesMoreTorque()
    {
        var fullEntity = MakeEntity(strength: 1000);
        var partialEntity = MakeEntity(strength: 1000);

        TurnInPlaceLocomotionStrategy.Execute(fullEntity, Direction.FromAngle(Math.PI / 2), forceFraction: 1.0, orientedThresholdRad: OrientedThresholdRad);
        TurnInPlaceLocomotionStrategy.Execute(partialEntity, Direction.FromAngle(Math.PI / 2), forceFraction: 0.5, orientedThresholdRad: OrientedThresholdRad);

        var fullTorque = fullEntity.PendingImpulses.Sum(v => v.Rx * v.Fy - v.Ry * v.Fx);
        var partialTorque = partialEntity.PendingImpulses.Sum(v => v.Rx * v.Fy - v.Ry * v.Fx);
        fullTorque.Should().BeGreaterThan(partialTorque);
    }
}
