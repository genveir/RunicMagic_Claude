using RunicMagic.Controller.Services;
using RunicMagic.World;
using RunicMagic.World.Entities.AI;
using RunicMagic.World.Entities.Capabilities;
using RunicMagic.World.Geometry;

namespace RunicMagic.Tests.World.Entities.AI;

public class PatrolAIBehaviorTests
{
    private static LocomotionCapability MakeLocomotion()
    {
        var legs = new List<Leg>
        {
            new Leg(name: "Left", lateralOffset: -50, forwardOffset: 0),
            new Leg(name: "Right", lateralOffset: 50, forwardOffset: 0)
        };
        return new LocomotionCapability(legs, locomotionEfficiency: 0.8);
    }

    private static PatrolWaypoint Waypoint(long x, long y, long waitTicks = 0)
    {
        return new PatrolWaypoint(new Location(x, y), waitTicks);
    }

    // ── No-op cases ───────────────────────────────────────────────────────────

    [Fact]
    public void Execute_DoesNothing_WhenLocomotionIsNull()
    {
        var entity = new EntityBuilder()
            .WithLocation(x: 0, y: 0)
            .WithStrength(10000)
            .Build();
        var behavior = new PatrolAIBehavior([Waypoint(10000, 0)], speed: 0.7);
        var world = new WorldModelBuilder().Build();
        var tracker = new EventTracker();

        behavior.Execute(entity, world, tracker, currentTick: 0);

        entity.PendingImpulses.Should().BeEmpty();
    }

    [Fact]
    public void Execute_DoesNothing_WhenNoWaypoints()
    {
        var entity = new EntityBuilder()
            .WithLocation(x: 0, y: 0)
            .WithStrength(10000)
            .WithLocomotion(MakeLocomotion())
            .Build();
        var behavior = new PatrolAIBehavior([], speed: 0.7);
        var world = new WorldModelBuilder().Build();
        var tracker = new EventTracker();

        behavior.Execute(entity, world, tracker, currentTick: 0);

        entity.PendingImpulses.Should().BeEmpty();
    }

    // ── Movement ──────────────────────────────────────────────────────────────

    [Fact]
    public void Execute_AddsImpulses_WhenMovingTowardWaypoint()
    {
        var entity = new EntityBuilder()
            .WithLocation(x: 0, y: 0)
            .WithStrength(10000)
            .WithLocomotion(MakeLocomotion())
            .Build();
        var behavior = new PatrolAIBehavior([Waypoint(10000, 0)], speed: 0.7);
        var world = new WorldModelBuilder().Build();
        var tracker = new EventTracker();

        behavior.Execute(entity, world, tracker, currentTick: 0);

        entity.PendingImpulses.Should().NotBeEmpty();
    }

    [Fact]
    public void Execute_HigherSpeed_ProducesMoreForce()
    {
        var entity = new EntityBuilder()
            .WithLocation(x: 0, y: 0)
            .WithStrength(10000)
            .WithLocomotion(MakeLocomotion())
            .Build();
        var behaviorWalk = new PatrolAIBehavior([Waypoint(10000, 0)], speed: 0.7);
        var behaviorRun = new PatrolAIBehavior([Waypoint(10000, 0)], speed: 1.0);
        var world = new WorldModelBuilder().Build();
        var tracker = new EventTracker();

        behaviorWalk.Execute(entity, world, tracker, currentTick: 0);
        var walkForce = entity.PendingImpulses.Sum(i => Math.Abs(i.Fx) + Math.Abs(i.Fy));
        entity.PendingImpulses.Clear();

        behaviorRun.Execute(entity, world, tracker, currentTick: 0);
        var runForce = entity.PendingImpulses.Sum(i => Math.Abs(i.Fx) + Math.Abs(i.Fy));

        runForce.Should().BeGreaterThan(walkForce);
    }

    // ── Waypoint advancement ──────────────────────────────────────────────────

    [Fact]
    public void Execute_AdvancesToNextWaypoint_OnArrivalWithNoWait()
    {
        var waypointA = new Location(200, 0);
        var waypointB = new Location(10000, 0);
        var entity = new EntityBuilder()
            .WithLocation(waypointA)
            .WithStrength(10000)
            .WithLocomotion(MakeLocomotion())
            .Build();
        var behavior = new PatrolAIBehavior(
            [new PatrolWaypoint(waypointA, WaitTicks: 0), new PatrolWaypoint(waypointB, WaitTicks: 0)],
            speed: 0.7);
        var world = new WorldModelBuilder().Build();
        var tracker = new EventTracker();

        behavior.Execute(entity, world, tracker, currentTick: 0);

        var impulse = entity.PendingImpulses[0];
        impulse.Fx.Should().BeGreaterThan(0, "should be moving toward waypointB which is to the right");
    }

    [Fact]
    public void Execute_WrapsAround_FromLastWaypointToFirst()
    {
        var waypointA = new Location(0, 0);
        var waypointB = new Location(200, 0);
        var entity = new EntityBuilder()
            .WithLocation(waypointB)
            .WithStrength(10000)
            .WithLocomotion(MakeLocomotion())
            .Build();
        var behavior = new PatrolAIBehavior(
            [new PatrolWaypoint(waypointA, WaitTicks: 0), new PatrolWaypoint(waypointB, WaitTicks: 0)],
            speed: 0.7);
        var world = new WorldModelBuilder().Build();
        var tracker = new EventTracker();

        behavior.Execute(entity, world, tracker, currentTick: 0);
        entity.PendingImpulses.Clear();

        behavior.Execute(entity, world, tracker, currentTick: 1);

        var impulse = entity.PendingImpulses[0];
        impulse.Fx.Should().BeLessThan(0, "should be moving toward waypointA which is to the left");
    }

    // ── Wait behaviour ────────────────────────────────────────────────────────

    [Fact]
    public void Execute_DoesNotMove_WhileWaitingAtWaypoint()
    {
        var waypointA = new Location(200, 0);
        var waypointB = new Location(10000, 0);
        var entity = new EntityBuilder()
            .WithLocation(waypointA)
            .WithStrength(10000)
            .WithLocomotion(MakeLocomotion())
            .Build();
        var behavior = new PatrolAIBehavior(
            [new PatrolWaypoint(waypointA, WaitTicks: 60), new PatrolWaypoint(waypointB, WaitTicks: 0)],
            speed: 0.7);
        var world = new WorldModelBuilder().Build();
        var tracker = new EventTracker();

        behavior.Execute(entity, world, tracker, currentTick: 0);
        entity.PendingImpulses.Clear();

        behavior.Execute(entity, world, tracker, currentTick: 1);

        entity.PendingImpulses.Should().BeEmpty();
    }

    [Fact]
    public void Execute_AdvancesAfterWait_WhenWaitTicksElapsed()
    {
        var waypointA = new Location(200, 0);
        var waypointB = new Location(10000, 0);
        var entity = new EntityBuilder()
            .WithLocation(waypointA)
            .WithStrength(10000)
            .WithLocomotion(MakeLocomotion())
            .Build();
        var behavior = new PatrolAIBehavior(
            [new PatrolWaypoint(waypointA, WaitTicks: 10), new PatrolWaypoint(waypointB, WaitTicks: 0)],
            speed: 0.7);
        var world = new WorldModelBuilder().Build();
        var tracker = new EventTracker();

        behavior.Execute(entity, world, tracker, currentTick: 0);
        entity.PendingImpulses.Clear();

        behavior.Execute(entity, world, tracker, currentTick: 10);

        entity.PendingImpulses.Should().NotBeEmpty();
        var impulse = entity.PendingImpulses[0];
        impulse.Fx.Should().BeGreaterThan(0, "should now be moving toward waypointB");
    }
}
