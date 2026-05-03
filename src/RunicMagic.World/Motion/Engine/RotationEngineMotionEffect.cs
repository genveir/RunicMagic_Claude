using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Motion.Shared;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Motion.Engine;

public class RotationEngineMotionEffect : IEngineMotionEffect
{
    private readonly TemporalSpellContext _temporalContext;
    private readonly IEntitySet _entities;
    private readonly ILocation _origin;
    private readonly double _perTickTheta;
    private readonly long _totalRuneDegrees;
    private readonly string _effectName;
    private int _remainingTicks;

    public bool IsComplete => _remainingTicks == 0;

    public RotationEngineMotionEffect(
        SpellContext context,
        IEntitySet toMove,
        ILocation origin,
        double perTickTheta,
        long totalRuneDegrees,
        string effectName)
    {
        _temporalContext = new(context);
        _entities = toMove;
        _origin = origin;
        _perTickTheta = perTickTheta;
        _totalRuneDegrees = totalRuneDegrees;
        _effectName = effectName;
        _remainingTicks = 56;
    }

    public EngineMotionEffectResult TryAdvance(IWorldEventTracker eventTracker)
    {
        var context = _temporalContext.ToSpellContext(eventTracker);

        var entities = _entities.Resolve(context);
        var origin = _origin.Evaluate(context);

        var totalCost = 0L;
        foreach (var entity in entities.Entities)
        {
            totalCost += RotationCostCalculator.ComputeEntityCost(entity, origin, Math.Abs(_perTickTheta));
        }

        var drawn = context.DrawPower(totalCost);
        if (drawn < totalCost)
        {
            eventTracker.Add(new EffectNotFiredEvent(_effectName, $"Insufficient power: needed {totalCost}, drew {drawn}"));
            return new(Advanced: false, EntitiesUnderMotion: []);
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
            MoveEntityService.Move(entity, newLocation, newAngle, eventTracker);
        }

        _remainingTicks--;

        if (_remainingTicks == 0 && _totalRuneDegrees > 0)
        {
            foreach (var entity in entities.Entities)
            {
                eventTracker.Add(new EntityRotatedEvent(entity, _totalRuneDegrees));
            }
        }

        return new(Advanced: true, EntitiesUnderMotion: entities.Entities);
    }
}
