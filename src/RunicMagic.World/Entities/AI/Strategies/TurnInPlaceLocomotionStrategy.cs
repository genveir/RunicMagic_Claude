using RunicMagic.World.Geometry;

namespace RunicMagic.World.Entities.AI.Strategies;

public enum TurnInPlaceResult
{
    CannotMove,
    NotOriented,
    Oriented,
}

public static class TurnInPlaceLocomotionStrategy
{
    public static TurnInPlaceResult Execute(Entity entity, Direction targetDirection, double forceFraction, double orientedThresholdRad)
    {
        if (entity.Locomotion == null)
        {
            return TurnInPlaceResult.CannotMove;
        }
        var locomotion = entity.Locomotion;

        var facing = Direction.FromAngle(entity.FacingAngle);
        var angularError = facing.AngularDifferenceTo(targetDirection);

        if (Math.Abs(angularError) < orientedThresholdRad && entity.Velocity.Omega == 0.0)
        {
            return TurnInPlaceResult.Oriented;
        }

        var (_, brakingAngle) = locomotion.GetAngularStopInfo(entity, forceFraction);
        var shouldBrake = brakingAngle >= Math.Abs(angularError);
        var shouldCoast = !shouldBrake && Math.Abs(entity.Velocity.Omega) + brakingAngle >= Math.Abs(angularError);

        if (shouldCoast)
        {
            return TurnInPlaceResult.NotOriented;
        }

        if (shouldBrake)
        {
            locomotion.ApplyAngularBrake(entity, forceFraction);
        }
        else
        {
            locomotion.ApplyTurnForce(entity, Math.Sign(angularError) * forceFraction);
        }

        return TurnInPlaceResult.NotOriented;
    }
}
