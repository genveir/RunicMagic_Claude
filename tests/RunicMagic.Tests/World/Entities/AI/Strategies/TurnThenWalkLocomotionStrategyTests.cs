using RunicMagic.Controller.Services;
using RunicMagic.World;
using RunicMagic.World.Entities;
using RunicMagic.World.Entities.AI.Strategies;
using RunicMagic.World.Entities.Capabilities;
using RunicMagic.World.Geometry;
using RunicMagic.World.Motion.Simulated;

namespace RunicMagic.Tests.World.Entities.AI.Strategies;

public class TurnThenWalkLocomotionStrategyTests
{
    private static LocomotionCapability MakeLocomotion()
    {
        var legs = new List<Leg>
        {
            new Leg(name: "Left", lateralOffset: -100, forwardOffset: 0),
            new Leg(name: "Right", lateralOffset: 100, forwardOffset: 0),
        };
        return new LocomotionCapability(legs, locomotionEfficiency: 1.0);
    }

    private static Entity MakeEntity(long x = 0, long y = 0, double angle = 0, long strength = 1000, long weight = 1000)
    {
        return new EntityBuilder()
            .WithLocation(x, y)
            .WithStrength(strength)
            .WithWeight(weight)
            .WithSize(200, 200)
            .WithAngle(angle)
            .Build();
    }

    private static readonly IWorldEventTracker Tracker = new EventTracker();

    // --- Arrival ---

    [Fact]
    public void Execute_WithinArrivalThreshold_AndStopped_AddsNoImpulses()
    {
        var locomotion = MakeLocomotion();
        var entity = MakeEntity();

        // 50 mm away is below the 100 mm arrival threshold; entity is already stopped
        TurnThenWalkLocomotionStrategy.Execute(entity, locomotion, new Location(50, 0), forceFraction: 1.0, Tracker);

        entity.PendingImpulses.Should().BeEmpty();
    }

    [Fact]
    public void Execute_WithinArrivalThreshold_AndMoving_AddsBrakingImpulses()
    {
        var locomotion = MakeLocomotion();
        var entity = MakeEntity(angle: 0);
        entity.Velocity = new VelocityVector(Vx: 100, Vy: 0, Omega: 0);

        TurnThenWalkLocomotionStrategy.Execute(entity, locomotion, new Location(50, 0), forceFraction: 1.0, Tracker);

        entity.PendingImpulses.Should().NotBeEmpty();
        entity.PendingImpulses.Sum(v => v.Fx).Should().BeLessThan(0);
    }

    // --- Turning phase ---

    [Fact]
    public void Execute_WhenMisaligned_AddsTurningImpulses()
    {
        var locomotion = MakeLocomotion();
        var entity = MakeEntity(); // facing right (angle = 0)

        // Target is 90° off to the left: should turn, not walk
        TurnThenWalkLocomotionStrategy.Execute(entity, locomotion, new Location(0, 10000), forceFraction: 1.0, Tracker);

        entity.PendingImpulses.Should().NotBeEmpty();
    }

    [Fact]
    public void Execute_TurningPhase_CoastsWhenOmegaAloneReachesTarget()
    {
        // Near-weightless entity: braking is instantaneous (brakingAngle ≈ 0).
        // omega (0.1) already exceeds the angular error (0.05), so no further impulse is needed.
        var legs = new List<Leg>
        {
            new Leg(name: "Left", lateralOffset: -500, forwardOffset: 0),
            new Leg(name: "Right", lateralOffset: 500, forwardOffset: 0),
        };
        var locomotion = new LocomotionCapability(legs, locomotionEfficiency: 1.0);
        var entity = new EntityBuilder()
            .WithStrength(100000)
            .WithWeight(1)
            .WithSize(200, 200)
            .Build();
        entity.Velocity = new VelocityVector(Vx: 0, Vy: 0, Omega: 0.1);

        // Target is just slightly to the left (angular error ≈ 0.05 rad < omega)
        var targetX = (long)(10000.0 * Math.Cos(0.05));
        var targetY = (long)(10000.0 * Math.Sin(0.05));
        TurnThenWalkLocomotionStrategy.Execute(entity, locomotion, new Location(targetX, targetY), forceFraction: 1.0, Tracker);

        entity.PendingImpulses.Should().BeEmpty();
    }

    [Fact]
    public void Execute_TurningPhase_AddsBrakingImpulses_WhenOmegaWouldOvershoottTarget()
    {
        var locomotion = MakeLocomotion();
        var entity = MakeEntity(strength: 1000, weight: 1000);
        entity.Velocity = new VelocityVector(Vx: 0, Vy: 0, Omega: 2.0);

        // Tiny angular error: entity would overshoot without braking
        var targetX = (long)(10000.0 * Math.Cos(0.01));
        var targetY = (long)(10000.0 * Math.Sin(0.01));
        TurnThenWalkLocomotionStrategy.Execute(entity, locomotion, new Location(targetX, targetY), forceFraction: 1.0, Tracker);

        entity.PendingImpulses.Should().NotBeEmpty();
    }

    // --- Walking phase ---

    [Fact]
    public void Execute_WhenAlignedAndFarAway_AddsForwardImpulses()
    {
        var locomotion = MakeLocomotion();
        var entity = MakeEntity(strength: 1000); // facing right (angle = 0)

        TurnThenWalkLocomotionStrategy.Execute(entity, locomotion, new Location(10000, 0), forceFraction: 1.0, Tracker);

        entity.PendingImpulses.Should().NotBeEmpty();
        entity.PendingImpulses.Sum(v => v.Fx).Should().BeGreaterThan(0);
    }

    [Fact]
    public void Execute_WalkingPhase_CoastsWhenSpeedPlusBrakingDistanceReachesTarget()
    {
        // Near-weightless entity: braking distance ≈ 0. Speed (150) equals remaining distance (150 mm) → coast.
        var locomotion = MakeLocomotion();
        var entity = new EntityBuilder()
            .WithLocation(0, 0)
            .WithWeight(1)
            .WithSize(200, 200)
            .Build();
        entity.Velocity = new VelocityVector(Vx: 150, Vy: 0, Omega: 0);

        TurnThenWalkLocomotionStrategy.Execute(entity, locomotion, new Location(150, 0), forceFraction: 1.0, Tracker);

        entity.PendingImpulses.Should().BeEmpty();
    }

    [Fact]
    public void Execute_WalkingPhase_AddsBrakingImpulses_WhenTooFastToStop()
    {
        // Entity moving at 5000 mm/tick; braking distance >> 200 mm destination
        var locomotion = MakeLocomotion();
        var entity = MakeEntity();
        entity.Velocity = new VelocityVector(Vx: 5000, Vy: 0, Omega: 0);

        TurnThenWalkLocomotionStrategy.Execute(entity, locomotion, new Location(200, 0), forceFraction: 1.0, Tracker);

        entity.PendingImpulses.Should().NotBeEmpty();
        entity.PendingImpulses.Sum(v => v.Fx).Should().BeLessThan(0);
    }

    // --- Force fraction ---

    [Fact]
    public void Execute_HigherForceFraction_ProducesMoreForce()
    {
        var locomotion = MakeLocomotion();
        var fullEntity = MakeEntity(strength: 1000);
        var partialEntity = MakeEntity(strength: 1000);

        TurnThenWalkLocomotionStrategy.Execute(fullEntity, locomotion, new Location(10000, 0), forceFraction: 1.0, Tracker);
        TurnThenWalkLocomotionStrategy.Execute(partialEntity, locomotion, new Location(10000, 0), forceFraction: 0.5, Tracker);

        var fullFx = fullEntity.PendingImpulses.Sum(v => v.Fx);
        var partialFx = partialEntity.PendingImpulses.Sum(v => v.Fx);
        fullFx.Should().BeGreaterThan(partialFx);
    }
}
