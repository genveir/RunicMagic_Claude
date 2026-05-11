using RunicMagic.Controller.Services;
using RunicMagic.World;
using RunicMagic.World.Entities;
using RunicMagic.World.Entities.AI.Behaviors;
using RunicMagic.World.Geometry;

namespace RunicMagic.Tests.World.Entities.AI;

public class PatrolAIBehaviorTests
{
    private static Entity MakeEntity(long x = 0, long y = 0, double angle = 0)
    {
        var entity = new EntityBuilder()
            .WithLocation(x, y)
            .WithStrength(1000)
            .WithWeight(1000)
            .WithSize(200, 200)
            .WithAngle(angle)
            .WithLocomotion(efficiency: 1.0)
            .Build();

        return entity;
    }

    private static WorldModel MakeWorld() => new WorldModelBuilder().Build();
    private static EventTracker MakeTracker() => new EventTracker();

    // --- No locomotion / no waypoints ---

    [Fact]
    public void Execute_WithNoLocomotion_AddsNoImpulses()
    {
        var entity = new EntityBuilder()
            .WithLocation(0, 0)
            .WithSize(200, 200)
            .Build();
        // entity.Locomotion is null

        var behavior = new PatrolAIBehavior(
            [new PatrolWaypoint(new Location(10000, 0), WaitTicks: 0, Facing: null)],
            speed: 1.0);

        behavior.Execute(entity, MakeWorld(), MakeTracker(), currentTick: 0);

        entity.PendingImpulses.Should().BeEmpty();
    }

    [Fact]
    public void Execute_WithNoWaypoints_AddsNoImpulses()
    {
        var entity = MakeEntity();
        var behavior = new PatrolAIBehavior([], speed: 1.0);

        behavior.Execute(entity, MakeWorld(), MakeTracker(), currentTick: 0);

        entity.PendingImpulses.Should().BeEmpty();
    }

    // --- Moving phase ---

    [Fact]
    public void Execute_WhenMovingToWaypoint_AddsImpulses()
    {
        var entity = MakeEntity();
        var behavior = new PatrolAIBehavior(
            [new PatrolWaypoint(new Location(10000, 0), WaitTicks: 0, Facing: null)],
            speed: 1.0);

        behavior.Execute(entity, MakeWorld(), MakeTracker(), currentTick: 0);

        entity.PendingImpulses.Should().NotBeEmpty();
    }

    // --- Waypoint arrival with no wait and no facing: advance to next waypoint ---

    [Fact]
    public void Execute_WhenArrivedAtWaypoint_NoWait_NoFacing_AdvancesToNextWaypoint()
    {
        // Entity is already at the first waypoint; next waypoint is to the right
        var entity = MakeEntity(x: 0, y: 0);
        var behavior = new PatrolAIBehavior(
            [
                new PatrolWaypoint(new Location(0, 0), WaitTicks: 0, Facing: null),
                new PatrolWaypoint(new Location(10000, 0), WaitTicks: 0, Facing: null),
            ],
            speed: 1.0);

        // First tick: arrives at waypoint 0, advances to waypoint 1
        behavior.Execute(entity, MakeWorld(), MakeTracker(), currentTick: 0);
        entity.PendingImpulses.Clear();

        // Second tick: should now be moving toward waypoint 1
        behavior.Execute(entity, MakeWorld(), MakeTracker(), currentTick: 1);

        entity.PendingImpulses.Should().NotBeEmpty();
    }

    // --- Arrival → waiting ---

    [Fact]
    public void Execute_WhenArrivedAtWaypoint_WithWait_NoFacing_EntersWaitState_AddsNoImpulses()
    {
        var entity = MakeEntity(x: 0, y: 0);
        var behavior = new PatrolAIBehavior(
            [new PatrolWaypoint(new Location(0, 0), WaitTicks: 5, Facing: null)],
            speed: 1.0);

        // First tick: arrives, transitions to Waiting
        behavior.Execute(entity, MakeWorld(), MakeTracker(), currentTick: 0);
        entity.PendingImpulses.Clear();

        // Second tick: still waiting, no impulses
        behavior.Execute(entity, MakeWorld(), MakeTracker(), currentTick: 1);

        entity.PendingImpulses.Should().BeEmpty();
    }

    [Fact]
    public void Execute_AfterWaitExpires_AdvancesToNextWaypoint()
    {
        var entity = MakeEntity(x: 0, y: 0);
        var behavior = new PatrolAIBehavior(
            [
                new PatrolWaypoint(new Location(0, 0), WaitTicks: 3, Facing: null),
                new PatrolWaypoint(new Location(10000, 0), WaitTicks: 0, Facing: null),
            ],
            speed: 1.0);

        // Tick 0: arrives at waypoint 0, transitions to Waiting (arrivedAtTick = 0)
        behavior.Execute(entity, MakeWorld(), MakeTracker(), currentTick: 0);
        entity.PendingImpulses.Clear();

        // Ticks 1-2: still waiting (3 ticks not elapsed yet)
        behavior.Execute(entity, MakeWorld(), MakeTracker(), currentTick: 1);
        entity.PendingImpulses.Should().BeEmpty();
        behavior.Execute(entity, MakeWorld(), MakeTracker(), currentTick: 2);
        entity.PendingImpulses.Should().BeEmpty();

        // Tick 3: wait expired (3 - 0 >= 3), advances to waypoint 1
        behavior.Execute(entity, MakeWorld(), MakeTracker(), currentTick: 3);
        entity.PendingImpulses.Clear();

        // Tick 4: now in Moving state toward waypoint 1
        behavior.Execute(entity, MakeWorld(), MakeTracker(), currentTick: 4);

        entity.PendingImpulses.Should().NotBeEmpty();
    }

    // --- Arrival → orienting ---

    [Fact]
    public void Execute_WhenArrivedAtWaypoint_WithFacing_EntersOrientingState()
    {
        var entity = MakeEntity(x: 0, y: 0, angle: 0);
        var behavior = new PatrolAIBehavior(
            [new PatrolWaypoint(new Location(0, 0), WaitTicks: 5, Facing: Math.PI / 2)],
            speed: 1.0);

        // First tick: arrives, transitions to Orienting
        behavior.Execute(entity, MakeWorld(), MakeTracker(), currentTick: 0);
        entity.PendingImpulses.Clear();

        // Second tick: orienting toward PI/2 while facing 0 — should add turning impulses
        behavior.Execute(entity, MakeWorld(), MakeTracker(), currentTick: 1);

        entity.PendingImpulses.Should().NotBeEmpty();
    }

    [Fact]
    public void Execute_WhenOriented_WithWait_EntersWaitState()
    {
        // Entity already facing the target direction
        var entity = MakeEntity(x: 0, y: 0, angle: Math.PI / 2);
        var behavior = new PatrolAIBehavior(
            [new PatrolWaypoint(new Location(0, 0), WaitTicks: 3, Facing: Math.PI / 2)],
            speed: 1.0);

        // Tick 0: arrives at waypoint, transitions to Orienting
        behavior.Execute(entity, MakeWorld(), MakeTracker(), currentTick: 0);
        entity.PendingImpulses.Clear();

        // Tick 1: already oriented (facing matches), transitions to Waiting
        behavior.Execute(entity, MakeWorld(), MakeTracker(), currentTick: 1);
        entity.PendingImpulses.Clear();

        // Tick 2: waiting, no impulses
        behavior.Execute(entity, MakeWorld(), MakeTracker(), currentTick: 2);

        entity.PendingImpulses.Should().BeEmpty();
    }

    // --- Wrapping ---

    [Fact]
    public void Execute_WrapsAroundWaypointList()
    {
        var entity = MakeEntity(x: 0, y: 0);
        var behavior = new PatrolAIBehavior(
            [
                new PatrolWaypoint(new Location(0, 0), WaitTicks: 0, Facing: null),
                new PatrolWaypoint(new Location(0, 0), WaitTicks: 0, Facing: null),
            ],
            speed: 1.0);

        // Each Execute at location (0,0) counts as arrived and advances the index
        // After two advances it wraps back to 0 — a third call should still work without throwing
        behavior.Execute(entity, MakeWorld(), MakeTracker(), currentTick: 0);
        behavior.Execute(entity, MakeWorld(), MakeTracker(), currentTick: 1);
        var act = () => behavior.Execute(entity, MakeWorld(), MakeTracker(), currentTick: 2);

        act.Should().NotThrow();
    }
}
