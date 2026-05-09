using RunicMagic.World.Entities.Capabilities;
using RunicMagic.World.Geometry;

namespace RunicMagic.World.Entities.AI.Strategies;

public static class TurnThenWalkLocomotionStrategy
{
    private const double ArrivalThresholdMm = 100.0;
    private const double StraightThresholdRad = 0.05;

    public static void Execute(Entity entity, LocomotionCapability locomotion, Location destination, double forceFraction, IWorldEventTracker tracker)
    {
        var dx = destination.X - entity.Location.X;
        var dy = destination.Y - entity.Location.Y;
        var remainingDistance = Math.Sqrt(dx * dx + dy * dy);

        if (remainingDistance < ArrivalThresholdMm)
        {
            if (entity.Velocity.Speed > 0.0)
            {
                locomotion.ApplyLinearBrake(entity, forceFraction);
            }
            return;
        }

        var desiredDirection = Direction.FromPoints(entity.Location, destination);
        var facing = Direction.FromAngle(entity.FacingAngle);
        var angularError = facing.AngularDifferenceTo(desiredDirection);

        if (Math.Abs(angularError) >= StraightThresholdRad || entity.Velocity.Omega != 0.0)
        {
            var (_, brakingAngle) = locomotion.GetAngularStopInfo(entity, forceFraction);
            var shouldBrake = brakingAngle >= Math.Abs(angularError);
            var shouldCoast = !shouldBrake && Math.Abs(entity.Velocity.Omega) + brakingAngle >= Math.Abs(angularError);
            if (shouldCoast)
            {
                return;
            }

            if (shouldBrake)
            {
                locomotion.ApplyAngularBrake(entity, forceFraction);
            }
            else
            {
                locomotion.ApplyTurnForce(entity, Math.Sign(angularError) * forceFraction);
            }
            return;
        }

        var (_, brakingDistance) = locomotion.GetLinearStopInfo(entity, forceFraction);
        var shouldBrakeLinear = brakingDistance >= remainingDistance;
        var shouldCoastLinear = !shouldBrakeLinear && entity.Velocity.Speed + brakingDistance >= remainingDistance;

        if (shouldCoastLinear)
        {
            return;
        }

        if (shouldBrakeLinear)
        {
            locomotion.ApplyLinearBrake(entity, forceFraction);
        }
        else
        {
            locomotion.ApplyLinearForce(entity, forceFraction);
        }
    }
}
