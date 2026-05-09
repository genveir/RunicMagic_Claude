using RunicMagic.World.Entities.AI.Strategies;
using RunicMagic.World.Geometry;

namespace RunicMagic.World.Entities.AI;

public class PatrolAIBehavior : IAIBehavior
{
    private const double ArrivalThresholdMm = 500.0;

    private readonly IReadOnlyList<PatrolWaypoint> waypoints;
    private readonly double speed;
    private int currentWaypointIndex;
    private long? arrivedAtTick;

    public PatrolAIBehavior(IReadOnlyList<PatrolWaypoint> waypoints, double speed)
    {
        this.waypoints = waypoints;
        this.speed = speed;
        this.currentWaypointIndex = 0;
        this.arrivedAtTick = null;
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

        if (arrivedAtTick.HasValue)
        {
            var waitTicks = waypoints[currentWaypointIndex].WaitTicks;
            if (currentTick - arrivedAtTick.Value < waitTicks)
            {
                return;
            }

            arrivedAtTick = null;
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
        }

        var waypoint = waypoints[currentWaypointIndex];
        var dx = waypoint.Location.X - entity.Location.X;
        var dy = waypoint.Location.Y - entity.Location.Y;
        var distance = Math.Sqrt(dx * dx + dy * dy);

        if (distance < ArrivalThresholdMm)
        {
            if (waypoint.WaitTicks > 0)
            {
                arrivedAtTick = currentTick;
                return;
            }

            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
            waypoint = waypoints[currentWaypointIndex];
        }

        TurnThenWalkLocomotionStrategy.Execute(entity, entity.Locomotion, waypoint.Location, speed, eventTracker);
    }
}
