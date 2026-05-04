using RunicMagic.Controller.Services;
using RunicMagic.World.Motion.Simulated;

namespace RunicMagic.Tests.World.Motion.Simulated;

public class PhysicsServiceTests
{
    [Fact]
    public void Tick_WithNoVelocityAndNoImpulses_EntityDoesNotMove()
    {
        var entity = new EntityBuilder().WithLocation(x: 0, y: 0).Build();
        var tracker = new EventTracker();

        PhysicsService.Tick([entity], tracker);

        entity.Location.X.Should().BeApproximately(0, 0.001);
        entity.Location.Y.Should().BeApproximately(0, 0.001);
        entity.Velocity.Should().BeNull();
    }

    [Fact]
    public void Tick_WithExistingVelocity_MovesEntityByVelocityEachTick()
    {
        var entity = new EntityBuilder().WithLocation(x: 0, y: 0).Build();
        entity.Velocity = new VelocityVector(10, 0);

        PhysicsService.Tick([entity], new EventTracker());

        entity.Location.X.Should().BeApproximately(10, 0.001);
        entity.Location.Y.Should().BeApproximately(0, 0.001);
    }

    [Fact]
    public void Tick_WithPendingImpulse_AcceleratesEntityByForceOverWeight()
    {
        // weight=1000, impulse=(1000,0) → Δv=(1,0), entity moves to (1,0)
        var entity = new EntityBuilder().WithLocation(x: 0, y: 0).WithWeight(1000).Build();
        entity.PendingImpulses.Add(new ForceVector(1000, 0));

        PhysicsService.Tick([entity], new EventTracker());

        entity.Velocity!.Value.Vx.Should().BeApproximately(1, 0.001);
        entity.Velocity!.Value.Vy.Should().BeApproximately(0, 0.001);
        entity.Location.X.Should().BeApproximately(1, 0.001);
    }

    [Fact]
    public void Tick_WithMultipleImpulses_SumsForces()
    {
        // Two impulses of (500,0) each → net (1000,0) → Δv=(1,0)
        var entity = new EntityBuilder().WithLocation(x: 0, y: 0).WithWeight(1000).Build();
        entity.PendingImpulses.Add(new ForceVector(500, 0));
        entity.PendingImpulses.Add(new ForceVector(500, 0));

        PhysicsService.Tick([entity], new EventTracker());

        entity.Velocity!.Value.Vx.Should().BeApproximately(1, 0.001);
    }

    [Fact]
    public void Tick_ClearsPendingImpulsesAfterProcessing()
    {
        var entity = new EntityBuilder().WithWeight(1000).Build();
        entity.PendingImpulses.Add(new ForceVector(1000, 0));

        PhysicsService.Tick([entity], new EventTracker());

        entity.PendingImpulses.Should().BeEmpty();
    }

    [Fact]
    public void Tick_EntityUnderEngineMotion_IsNotMoved()
    {
        var entity = new EntityBuilder().WithLocation(x: 0, y: 0).Build();
        entity.IsUnderEngineMotion = true;
        entity.Velocity = new VelocityVector(10, 0);

        PhysicsService.Tick([entity], new EventTracker());

        entity.Location.X.Should().BeApproximately(0, 0.001);
    }

    [Fact]
    public void Tick_EntityUnderEngineMotion_ClearsPendingImpulses()
    {
        var entity = new EntityBuilder().Build();
        entity.IsUnderEngineMotion = true;
        entity.PendingImpulses.Add(new ForceVector(1000, 0));

        PhysicsService.Tick([entity], new EventTracker());

        entity.PendingImpulses.Should().BeEmpty();
    }

    [Fact]
    public void Tick_WithDragCoefficient_ReducesVelocity()
    {
        // 100x100 entity, weight=1000, drag=0.01, velocity=(100,0)
        // projected width in +X = height = 100
        // drag_force = 0.01 × 100 × 100² = 10000
        // drag_decel = 10000/1000 = 10 → new speed = 90
        var entity = new EntityBuilder()
            .WithLocation(x: 0, y: 0)
            .WithSize(width: 100, height: 100)
            .WithWeight(1000)
            .WithDragCoefficient(0.01)
            .Build();
        entity.Velocity = new VelocityVector(100, 0);

        PhysicsService.Tick([entity], new EventTracker());

        entity.Velocity!.Value.Vx.Should().BeApproximately(90, 0.001);
        entity.Location.X.Should().BeApproximately(90, 0.001);
    }

    [Fact]
    public void Tick_WhenSpeedFallsBelowThreshold_NullsVelocity()
    {
        // velocity (0.5, 0) is below the 1mm/tick minimum
        var entity = new EntityBuilder().WithLocation(x: 0, y: 0).Build();
        entity.Velocity = new VelocityVector(0.5, 0);

        PhysicsService.Tick([entity], new EventTracker());

        entity.Velocity.Should().BeNull();
        entity.Location.X.Should().BeApproximately(0, 0.001);
    }

    [Fact]
    public void Tick_WhenDragBringsSpeedBelowThreshold_NullsVelocity()
    {
        // velocity (1.0, 0), drag large enough to kill it in one tick
        // weight=1000, size=100x100, drag=1.0
        // drag_force = 1.0 × 100 × 1² = 100 → drag_decel = 0.1 → new speed = 0.9 < 1.0
        var entity = new EntityBuilder()
            .WithSize(width: 100, height: 100)
            .WithWeight(1000)
            .WithDragCoefficient(1.0)
            .Build();
        entity.Velocity = new VelocityVector(1.0, 0);

        PhysicsService.Tick([entity], new EventTracker());

        entity.Velocity.Should().BeNull();
    }

    [Fact]
    public void Tick_ImpulseAddsToExistingVelocity()
    {
        // existing velocity (5,0), impulse (1000,0) → Δv=(1,0), new velocity=(6,0)
        var entity = new EntityBuilder().WithLocation(x: 0, y: 0).WithWeight(1000).Build();
        entity.Velocity = new VelocityVector(5, 0);
        entity.PendingImpulses.Add(new ForceVector(1000, 0));

        PhysicsService.Tick([entity], new EventTracker());

        entity.Velocity!.Value.Vx.Should().BeApproximately(6, 0.001);
    }

    [Fact]
    public void Tick_TracksEntityWhenMoved()
    {
        var entity = new EntityBuilder().Build();
        entity.Velocity = new VelocityVector(10, 0);
        var tracker = new EventTracker();

        PhysicsService.Tick([entity], tracker);

        tracker.TouchedEntities.Should().Contain(entity);
    }

    [Fact]
    public void Tick_DoesNotTrackEntityThatDoesNotMove()
    {
        var entity = new EntityBuilder().Build();
        var tracker = new EventTracker();

        PhysicsService.Tick([entity], tracker);

        tracker.TouchedEntities.Should().NotContain(entity);
    }

    [Fact]
    public void Tick_OffCentreImpulse_ProducesAngularVelocity()
    {
        // weight=12000, size=100x100 → I = 12000 * (100² + 100²) / 12 = 20,000,000
        // force (0, 20,000,000) at offset (1, 0) → torque = 1 * 20,000,000 = 20,000,000
        // angular accel = 20,000,000 / 20,000,000 = 1.0 rad/tick
        var entity = new EntityBuilder()
            .WithSize(width: 100, height: 100)
            .WithWeight(12000)
            .Build();
        entity.PendingImpulses.Add(new ForceVector(Fx: 0, Fy: 20_000_000, Rx: 1, Ry: 0));

        PhysicsService.Tick([entity], new EventTracker());

        entity.Velocity!.Value.Omega.Should().BeApproximately(1.0, 0.001);
    }

    [Fact]
    public void Tick_OffCentreImpulse_StillAppliesFullForceToLinearVelocity()
    {
        // The full force goes into linear acceleration, not split with rotation
        // weight=12000, force (0, 12000) at offset (1, 0)
        // linear: vy += 12000 / 12000 = 1.0 mm/tick
        var entity = new EntityBuilder()
            .WithSize(width: 100, height: 100)
            .WithWeight(12000)
            .Build();
        entity.PendingImpulses.Add(new ForceVector(Fx: 0, Fy: 12000, Rx: 1, Ry: 0));

        PhysicsService.Tick([entity], new EventTracker());

        entity.Velocity!.Value.Vy.Should().BeApproximately(1.0, 0.001);
    }

    [Fact]
    public void Tick_CentreImpulse_ProducesNoTorque()
    {
        // Rx=0, Ry=0 → torque = 0 → no angular velocity
        var entity = new EntityBuilder()
            .WithSize(width: 100, height: 100)
            .WithWeight(12000)
            .Build();
        entity.PendingImpulses.Add(new ForceVector(Fx: 0, Fy: 12000, Rx: 0, Ry: 0));

        PhysicsService.Tick([entity], new EventTracker());

        entity.Velocity!.Value.Omega.Should().BeApproximately(0.0, 0.001);
    }

    [Fact]
    public void Tick_WithAngularVelocity_UpdatesEntityAngle()
    {
        var entity = new EntityBuilder().WithAngle(0).Build();
        entity.Velocity = new VelocityVector(Vx: 10, Vy: 0, Omega: 0.5);

        PhysicsService.Tick([entity], new EventTracker());

        entity.Angle.Should().BeApproximately(0.5, 0.001);
    }

    [Fact]
    public void Tick_WithAngularDrag_ReducesAngularVelocity()
    {
        // weight=12000, size=100x100 → I = 20,000,000
        // omega=1.0, angularDragCoefficient=10,000,000
        // angularDragTorque = 10,000,000 * 1.0² = 10,000,000
        // angularDragDecel = 10,000,000 / 20,000,000 = 0.5 → newOmega = 0.5
        var entity = new EntityBuilder()
            .WithSize(width: 100, height: 100)
            .WithWeight(12000)
            .WithAngularDragCoefficient(10_000_000)
            .Build();
        entity.Velocity = new VelocityVector(Vx: 10, Vy: 0, Omega: 1.0);

        PhysicsService.Tick([entity], new EventTracker());

        entity.Velocity!.Value.Omega.Should().BeApproximately(0.5, 0.001);
    }

    [Fact]
    public void Tick_WhenAngularSpeedFallsBelowThreshold_ZeroesOmega()
    {
        // omega=0.0005 is below MinAngularSpeedRadPerTick=0.001
        // linear velocity is above threshold so Velocity stays non-null, but Omega should be zeroed
        var entity = new EntityBuilder().Build();
        entity.Velocity = new VelocityVector(Vx: 10, Vy: 0, Omega: 0.0005);

        PhysicsService.Tick([entity], new EventTracker());

        entity.Velocity!.Value.Omega.Should().BeApproximately(0.0, 0.001);
    }

    [Fact]
    public void Tick_WhenBothLinearAndAngularBelowThreshold_NullsVelocity()
    {
        var entity = new EntityBuilder().Build();
        entity.Velocity = new VelocityVector(Vx: 0.5, Vy: 0, Omega: 0.0005);

        PhysicsService.Tick([entity], new EventTracker());

        entity.Velocity.Should().BeNull();
    }

    [Fact]
    public void Tick_PureAngularVelocity_TracksEntity()
    {
        // Entity with only spin and no linear movement should still be tracked
        var entity = new EntityBuilder().Build();
        entity.Velocity = new VelocityVector(Vx: 0, Vy: 0, Omega: 0.1);
        var tracker = new EventTracker();

        PhysicsService.Tick([entity], tracker);

        tracker.TouchedEntities.Should().Contain(entity);
    }
}
