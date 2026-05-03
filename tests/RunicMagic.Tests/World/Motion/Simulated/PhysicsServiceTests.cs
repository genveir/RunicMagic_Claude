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
}
