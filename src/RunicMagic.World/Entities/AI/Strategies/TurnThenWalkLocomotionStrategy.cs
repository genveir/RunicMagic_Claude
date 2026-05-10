using RunicMagic.World.Geometry;

namespace RunicMagic.World.Entities.AI.Strategies;

public enum TurnThenWalkResult
{
    CannotMove,
    Arrived,
    NotArrived
}

public static class TurnThenWalkLocomotionStrategy
{
    public static TurnThenWalkResult Execute(Entity entity, Location destination, double forceFraction, double arrivalThresholdMm, double straightThresholdRad)
    {
        if (entity.Locomotion == null)
        {
            return TurnThenWalkResult.CannotMove;
        }
        var locomotion = entity.Locomotion;

        var dx = destination.X - entity.Location.X;
        var dy = destination.Y - entity.Location.Y;
        var remainingDistance = Math.Sqrt(dx * dx + dy * dy);

        if (remainingDistance < arrivalThresholdMm)
        {
            if (entity.Velocity.Speed > 0.0)
            {
                locomotion.ApplyLinearBrake(entity, forceFraction);
            }
            return TurnThenWalkResult.Arrived;
        }

        var desiredDirection = Direction.FromPoints(entity.Location, destination);
        var turnInPlaceResult = TurnInPlaceLocomotionStrategy.Execute(entity, desiredDirection, forceFraction, straightThresholdRad);

        if (turnInPlaceResult == TurnInPlaceResult.NotOriented)
        {
            return TurnThenWalkResult.NotArrived;
        }

        var (_, brakingDistance) = locomotion.GetLinearStopInfo(entity, forceFraction);
        var shouldBrake = brakingDistance >= remainingDistance;
        var shouldCoast = !shouldBrake && entity.Velocity.Speed + brakingDistance >= remainingDistance;

        if (shouldCoast)
        {
            return TurnThenWalkResult.NotArrived;
        }

        if (shouldBrake)
        {
            locomotion.ApplyLinearBrake(entity, forceFraction);
        }
        else
        {
            locomotion.ApplyLinearForce(entity, forceFraction);
        }

        return TurnThenWalkResult.NotArrived;
    }
}
