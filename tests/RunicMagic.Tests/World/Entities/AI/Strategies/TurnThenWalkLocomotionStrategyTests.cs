using RunicMagic.World.Entities;
using RunicMagic.World.Entities.AI.Strategies;
using RunicMagic.World.Geometry;
using RunicMagic.World.Motion.Simulated;

namespace RunicMagic.Tests.World.Entities.AI.Strategies;

public class TurnThenWalkLocomotionStrategyTests
{
    private const double ArrivalThresholdMm = 100.0;
    private const double StraightThresholdRad = 0.05;

    private static Entity MakeEntity(long x = 0, long y = 0, double angle = 0, long strength = 1000, long weight = 1000)
    {
        return new EntityBuilder()
            .WithLocation(x, y)
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

        var result = TurnThenWalkLocomotionStrategy.Execute(entity, new Location(10000, 0), forceFraction: 1.0, arrivalThresholdMm: ArrivalThresholdMm, straightThresholdRad: StraightThresholdRad);

        result.Should().Be(TurnThenWalkResult.CannotMove);
    }

    // --- Arrival ---

    [Fact]
    public void Execute_WithinArrivalThreshold_AndStopped_AddsNoImpulses()
    {
        var entity = MakeEntity();

        // 50 mm away is below the 100 mm arrival threshold; entity is already stopped
        TurnThenWalkLocomotionStrategy.Execute(entity, new Location(50, 0), forceFraction: 1.0, arrivalThresholdMm: ArrivalThresholdMm, straightThresholdRad: StraightThresholdRad);

        entity.PendingImpulses.Should().BeEmpty();
    }

    [Fact]
    public void Execute_WithinArrivalThreshold_AndMoving_AddsBrakingImpulses()
    {
        var entity = MakeEntity(angle: 0);
        entity.Velocity = new VelocityVector(Vx: 100, Vy: 0, Omega: 0);

        TurnThenWalkLocomotionStrategy.Execute(entity, new Location(50, 0), forceFraction: 1.0, arrivalThresholdMm: ArrivalThresholdMm, straightThresholdRad: StraightThresholdRad);

        entity.PendingImpulses.Should().NotBeEmpty();
        entity.PendingImpulses.Sum(v => v.Fx).Should().BeLessThan(0);
    }

    // --- Turning phase ---

    [Fact]
    public void Execute_WhenMisaligned_AddsTurningImpulses()
    {
        var entity = MakeEntity(); // facing right (angle = 0)

        // Target is 90° off to the left: should turn, not walk
        TurnThenWalkLocomotionStrategy.Execute(entity, new Location(0, 10000), forceFraction: 1.0, arrivalThresholdMm: ArrivalThresholdMm, straightThresholdRad: StraightThresholdRad);

        entity.PendingImpulses.Should().NotBeEmpty();
    }

    [Fact]
    public void Execute_TurningPhase_CoastsWhenOmegaAloneReachesTarget()
    {
        // Near-weightless entity: braking is instantaneous (brakingAngle ≈ 0).
        // omega (0.1) already exceeds the angular error (0.05), so no further impulse is needed.
        var entity = MakeEntity(strength: 100000, weight: 1);
        entity.Velocity = new VelocityVector(Vx: 0, Vy: 0, Omega: 0.1);

        // Target is just slightly to the left (angular error ≈ 0.05 rad < omega)
        var targetX = (long)(10000.0 * Math.Cos(0.05));
        var targetY = (long)(10000.0 * Math.Sin(0.05));
        TurnThenWalkLocomotionStrategy.Execute(entity, new Location(targetX, targetY), forceFraction: 1.0, arrivalThresholdMm: ArrivalThresholdMm, straightThresholdRad: StraightThresholdRad);

        entity.PendingImpulses.Should().BeEmpty();
    }

    [Fact]
    public void Execute_TurningPhase_AddsBrakingImpulses_WhenOmegaWouldOvershoottTarget()
    {
        var entity = MakeEntity(strength: 1000, weight: 1000);
        entity.Velocity = new VelocityVector(Vx: 0, Vy: 0, Omega: 2.0);

        // Tiny angular error: entity would overshoot without braking
        var targetX = (long)(10000.0 * Math.Cos(0.01));
        var targetY = (long)(10000.0 * Math.Sin(0.01));
        TurnThenWalkLocomotionStrategy.Execute(entity, new Location(targetX, targetY), forceFraction: 1.0, arrivalThresholdMm: ArrivalThresholdMm, straightThresholdRad: StraightThresholdRad);

        entity.PendingImpulses.Should().NotBeEmpty();
    }

    // --- Walking phase ---

    [Fact]
    public void Execute_WhenAlignedAndFarAway_AddsForwardImpulses()
    {
        var entity = MakeEntity(strength: 1000); // facing right (angle = 0)

        TurnThenWalkLocomotionStrategy.Execute(entity, new Location(10000, 0), forceFraction: 1.0, arrivalThresholdMm: ArrivalThresholdMm, straightThresholdRad: StraightThresholdRad);

        entity.PendingImpulses.Should().NotBeEmpty();
        entity.PendingImpulses.Sum(v => v.Fx).Should().BeGreaterThan(0);
    }

    [Fact]
    public void Execute_WalkingPhase_CoastsWhenSpeedPlusBrakingDistanceReachesTarget()
    {
        // Near-weightless entity: braking distance ≈ 0. Speed (150) equals remaining distance (150 mm) → coast.
        var entity = new EntityBuilder()
            .WithLocation(0, 0)
            .WithWeight(1)
            .WithSize(200, 200)
            .Build();
        entity.Velocity = new VelocityVector(Vx: 150, Vy: 0, Omega: 0);

        TurnThenWalkLocomotionStrategy.Execute(entity, new Location(150, 0), forceFraction: 1.0, arrivalThresholdMm: ArrivalThresholdMm, straightThresholdRad: StraightThresholdRad);

        entity.PendingImpulses.Should().BeEmpty();
    }

    [Fact]
    public void Execute_WalkingPhase_AddsBrakingImpulses_WhenTooFastToStop()
    {
        // Entity moving at 5000 mm/tick; braking distance >> 200 mm destination
        var entity = MakeEntity();
        entity.Velocity = new VelocityVector(Vx: 5000, Vy: 0, Omega: 0);

        TurnThenWalkLocomotionStrategy.Execute(entity, new Location(200, 0), forceFraction: 1.0, arrivalThresholdMm: ArrivalThresholdMm, straightThresholdRad: StraightThresholdRad);

        entity.PendingImpulses.Should().NotBeEmpty();
        entity.PendingImpulses.Sum(v => v.Fx).Should().BeLessThan(0);
    }

    // --- Force fraction ---

    [Fact]
    public void Execute_HigherForceFraction_ProducesMoreForce()
    {
        var fullEntity = MakeEntity(strength: 1000);
        var partialEntity = MakeEntity(strength: 1000);

        TurnThenWalkLocomotionStrategy.Execute(fullEntity, new Location(10000, 0), forceFraction: 1.0, arrivalThresholdMm: ArrivalThresholdMm, straightThresholdRad: StraightThresholdRad);
        TurnThenWalkLocomotionStrategy.Execute(partialEntity, new Location(10000, 0), forceFraction: 0.5, arrivalThresholdMm: ArrivalThresholdMm, straightThresholdRad: StraightThresholdRad);

        var fullFx = fullEntity.PendingImpulses.Sum(v => v.Fx);
        var partialFx = partialEntity.PendingImpulses.Sum(v => v.Fx);
        fullFx.Should().BeGreaterThan(partialFx);
    }
}
