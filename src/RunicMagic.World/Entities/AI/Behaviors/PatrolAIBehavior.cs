using RunicMagic.World.Entities.AI.Strategies;
using RunicMagic.World.Geometry;

namespace RunicMagic.World.Entities.AI.Behaviors;

public class PatrolAIBehavior : IAIBehavior
{
    private enum PatrolState
    {
        Moving,
        Orienting,
        Waiting
    }
    private PatrolState state = PatrolState.Moving;

    private enum Transition
    {
        NoTransition,
        Orient,
        Wait,
        IncrementWaypoint
    }

    private const double WaypointArrivalThresholdMm = 500.0;
    private const double StraightThresholdRad = 0.05;
    private const double OrientedThresholdRad = 0.05;

    private readonly IReadOnlyList<PatrolWaypoint> waypoints;
    private readonly double speed;
    private int currentWaypointIndex;

    private long arrivedAtTick;

    public PatrolAIBehavior(IReadOnlyList<PatrolWaypoint> waypoints, double speed)
    {
        this.waypoints = waypoints;
        this.speed = speed;
        this.currentWaypointIndex = 0;

        if (waypoints.Count > 0)
        {
            this.arrivedAtTick = -waypoints.Max(w => w.WaitTicks);
        }
    }

    public void Execute(Entity entity, WorldModel worldModel, IWorldEventTracker eventTracker, long currentTick)
    {
        if (entity.Locomotion == null)
        {
            return;
        }

        if (waypoints.Count == 0)
        {
            return;
        }

        var waypoint = waypoints[currentWaypointIndex];

        var nextAction = state switch
        {
            PatrolState.Moving => ExecuteMove(entity, waypoint),
            PatrolState.Orienting => ExecuteOrient(entity, waypoint),
            PatrolState.Waiting => ExecuteWait(currentTick),
            _ => throw new ArgumentException($"Invalid patrol state: {state}")
        };

        switch (nextAction)
        {
            case Transition.NoTransition:
                break;
            case Transition.Orient:
                ShiftToOrient();
                break;
            case Transition.Wait:
                ShiftToWait(currentTick);
                break;
            case Transition.IncrementWaypoint:
                IncrementWaypoint();
                break;
            default:
                throw new ArgumentException($"Invalid next action: {nextAction}");
        }
    }

    private void IncrementWaypoint()
    {
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
        state = PatrolState.Moving;
    }

    private void ShiftToWait(long currentTick)
    {
        arrivedAtTick = currentTick;
        state = PatrolState.Waiting;
    }

    private void ShiftToOrient()
    {
        state = PatrolState.Orienting;
    }

    private Transition ExecuteMove(Entity entity, PatrolWaypoint waypoint)
    {
        var turnThenWalkResult = TurnThenWalkLocomotionStrategy.Execute(
            entity: entity,
            destination: waypoint.Location,
            forceFraction: speed,
            arrivalThresholdMm: WaypointArrivalThresholdMm,
            straightThresholdRad: StraightThresholdRad);

        if (turnThenWalkResult is TurnThenWalkResult.NotArrived or TurnThenWalkResult.CannotMove)
        {
            return Transition.NoTransition;
        }

        if (waypoint.WaitTicks > 0)
        {
            if (waypoint.Facing.HasValue)
            {
                return Transition.Orient;
            }
            else
            {
                return Transition.Wait;
            }
        }
        else
        {
            return Transition.IncrementWaypoint;
        }
    }

    private Transition ExecuteOrient(Entity entity, PatrolWaypoint waypoint)
    {
        if (waypoint.Facing == null)
        {
            return Transition.Wait;
        }

        var targetDirection = Direction.FromAngle(waypoint.Facing.Value);

        var isOriented = TurnInPlaceLocomotionStrategy.Execute(entity, targetDirection, speed, OrientedThresholdRad);

        if (isOriented is TurnInPlaceResult.NotOriented or TurnInPlaceResult.CannotMove)
        {
            return Transition.NoTransition;
        }

        if (waypoint.WaitTicks > 0)
        {
            return Transition.Wait;
        }
        else
        {
            return Transition.IncrementWaypoint;
        }
    }

    private Transition ExecuteWait(long currentTick)
    {
        var waitTicks = waypoints[currentWaypointIndex].WaitTicks;
        if (currentTick - arrivedAtTick >= waitTicks)
        {
            return Transition.IncrementWaypoint;
        }

        return Transition.NoTransition;
    }
}
