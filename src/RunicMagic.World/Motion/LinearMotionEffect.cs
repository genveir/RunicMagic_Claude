using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Runes.RuneTypes;
using RunicMagic.World.Services;

namespace RunicMagic.World.Motion;

public class LinearMotionEffect : IMotionEffect
{
    private readonly TemporalSpellContext _temporalContext;
    private readonly IEntitySet _entities;
    private readonly ILocation _origin;
    private readonly double _perTickDistance;
    private readonly long _perTickCost;
    private readonly long _totalDistanceMm;
    private readonly bool _isAway;
    private readonly string _effectName;
    private int _remainingTicks;

    public bool IsComplete => _remainingTicks == 0;

    public LinearMotionEffect(
        SpellContext context,
        IEntitySet entities,
        ILocation origin,
        double perTickDistance,
        long perTickCost,
        long totalDistanceMm,
        bool isAway,
        string effectName)
    {
        _temporalContext = new(context);
        _entities = entities;
        _origin = origin;
        _perTickDistance = perTickDistance;
        _perTickCost = perTickCost;
        _totalDistanceMm = totalDistanceMm;
        _isAway = isAway;
        _effectName = effectName;
        _remainingTicks = 56;
    }

    public bool TryAdvance(IWorldEventTracker eventTracker)
    {
        var context = _temporalContext.ToSpellContext(eventTracker);

        var drawn = context.DrawPower(_perTickCost);
        if (drawn < _perTickCost)
        {
            eventTracker.Add(new EffectNotFiredEvent(_effectName, $"Insufficient power: needed {_perTickCost}, drew {drawn}"));
            return false;
        }

        var entities = _entities.Resolve(context);
        var origin = _origin.Evaluate(context);

        foreach (var entity in entities.Entities)
        {
            Direction direction;
            if (_isAway)
            {
                direction = Direction.FromPoints(origin, entity.Location);
            }
            else
            {
                direction = Direction.FromPoints(entity.Location, origin);
            }

            var destination = entity.Location.Translate(direction, _perTickDistance);
            MoveEntityService.Move(entity, destination, entity.Angle, eventTracker);
        }

        _remainingTicks--;

        if (_remainingTicks == 0 && _totalDistanceMm > 0)
        {
            foreach (var entity in entities.Entities)
            {
                if (_isAway)
                {
                    eventTracker.Add(new EntityPushedEvent(entity, _totalDistanceMm));
                }
                else
                {
                    eventTracker.Add(new EntityPulledEvent(entity, _totalDistanceMm));
                }
            }
        }

        return true;
    }
}
