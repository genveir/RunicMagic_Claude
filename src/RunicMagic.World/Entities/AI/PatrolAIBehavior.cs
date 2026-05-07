namespace RunicMagic.World.Entities.AI;

public class PatrolAIBehavior : IAIBehavior
{
    private const double ArrivalThresholdMm = 500.0;

    private readonly IReadOnlyList<PatrolWaypoint> _waypoints;
    private readonly double _speed;
    private int _currentWaypointIndex;
    private long? _arrivedAtTick;

    public PatrolAIBehavior(IReadOnlyList<PatrolWaypoint> waypoints, double speed)
    {
        _waypoints = waypoints;
        _speed = speed;
        _currentWaypointIndex = 0;
        _arrivedAtTick = null;
    }

    public void Execute(Entity entity, WorldModel worldModel, IWorldEventTracker eventTracker, long currentTick)
    {
        if (entity.Locomotion == null)
        {
            return;
        }

        if (_waypoints.Count == 0)
        {
            return;
        }

        if (_arrivedAtTick.HasValue)
        {
            var waitTicks = _waypoints[_currentWaypointIndex].WaitTicks;
            if (currentTick - _arrivedAtTick.Value < waitTicks)
            {
                return;
            }

            _arrivedAtTick = null;
            _currentWaypointIndex = (_currentWaypointIndex + 1) % _waypoints.Count;
        }

        var waypoint = _waypoints[_currentWaypointIndex];
        var dx = waypoint.Location.X - entity.Location.X;
        var dy = waypoint.Location.Y - entity.Location.Y;
        var distance = Math.Sqrt(dx * dx + dy * dy);

        if (distance < ArrivalThresholdMm)
        {
            if (waypoint.WaitTicks > 0)
            {
                _arrivedAtTick = currentTick;
                return;
            }

            _currentWaypointIndex = (_currentWaypointIndex + 1) % _waypoints.Count;
            waypoint = _waypoints[_currentWaypointIndex];
        }

        if (_speed >= 1.0)
        {
            entity.Locomotion.Run(entity, waypoint.Location, eventTracker);
        }
        else
        {
            entity.Locomotion.Walk(entity, waypoint.Location, eventTracker);
        }
    }
}
