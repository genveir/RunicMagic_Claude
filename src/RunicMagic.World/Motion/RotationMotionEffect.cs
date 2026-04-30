using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Runes.RuneTypes;
using RunicMagic.World.Services;

namespace RunicMagic.World.Motion;

public class RotationMotionEffect : IMotionEffect
{
    private readonly SpellContext _context;
    private readonly IEntitySet _entities;
    private readonly ILocation _origin;
    private readonly double _perTickTheta;
    private readonly long _totalRuneDegrees;
    private readonly string _effectName;
    private int _remainingTicks;

    public bool IsComplete => _remainingTicks == 0;

    public RotationMotionEffect(
        SpellContext context,
        IEntitySet entities,
        ILocation origin,
        double perTickTheta,
        long totalRuneDegrees,
        string effectName)
    {
        _context = context;
        _entities = entities;
        _origin = origin;
        _perTickTheta = perTickTheta;
        _totalRuneDegrees = totalRuneDegrees;
        _effectName = effectName;
        _remainingTicks = 56;
    }

    public bool TryAdvance(IWorldEventTracker tickResult)
    {
        var previousResult = _context.EventTracker;
        _context.EventTracker = tickResult;
        try
        {
            return TryAdvanceInner(tickResult);
        }
        finally
        {
            _context.EventTracker = previousResult;
        }
    }

    private bool TryAdvanceInner(IWorldEventTracker tickResult)
    {
        var entities = _entities.Resolve(_context);
        var origin = _origin.Evaluate(_context);

        var totalCost = 0L;
        foreach (var entity in entities.Entities)
        {
            totalCost += RotationCostCalculator.ComputeEntityCost(entity, origin, Math.Abs(_perTickTheta));
        }

        var drawn = _context.DrawPower(totalCost);
        if (drawn < totalCost)
        {
            tickResult.Add(new EffectNotFiredEvent(_effectName, $"Insufficient power: needed {totalCost}, drew {drawn}"));
            return false;
        }

        var cos = Math.Cos(_perTickTheta);
        var sin = Math.Sin(_perTickTheta);

        foreach (var entity in entities.Entities)
        {
            var dx = entity.Location.X - origin.X;
            var dy = entity.Location.Y - origin.Y;
            var newLocation = new Location(
                origin.X + dx * cos - dy * sin,
                origin.Y + dx * sin + dy * cos
            );
            var newAngle = entity.Angle + _perTickTheta;
            MoveEntityService.Move(entity, newLocation, newAngle, tickResult);
        }

        _remainingTicks--;

        if (_remainingTicks == 0 && _totalRuneDegrees > 0)
        {
            foreach (var entity in entities.Entities)
            {
                tickResult.Add(new EntityRotatedEvent(entity, _totalRuneDegrees));
            }
        }

        return true;
    }
}
