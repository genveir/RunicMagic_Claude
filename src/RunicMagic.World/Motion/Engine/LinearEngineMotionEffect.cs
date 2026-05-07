using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Motion.Shared;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Motion.Engine;

public class LinearEngineMotionEffect : IEngineMotionEffect
{
    private readonly TemporalSpellContext _temporalContext;
    private readonly IEntitySet _toMove;
    private readonly ILocation _origin;
    private readonly double _perTickDistance;
    private readonly long _totalDistanceMm;
    private readonly bool _isAway;
    private readonly string _effectName;
    private int _remainingTicks;

    public bool IsComplete => _remainingTicks == 0;

    public LinearEngineMotionEffect(
        SpellContext context,
        IEntitySet toMove,
        ILocation origin,
        double perTickDistance,
        long totalDistanceMm,
        bool isAway,
        string effectName)
    {
        _temporalContext = new(context);
        _toMove = toMove;
        _origin = origin;
        _perTickDistance = perTickDistance;
        _totalDistanceMm = totalDistanceMm;
        _isAway = isAway;
        _effectName = effectName;
        _remainingTicks = 56;
    }

    public EngineMotionEffectResult TryAdvance(IWorldEventTracker eventTracker)
    {
        var context = _temporalContext.ToSpellContext(eventTracker);

        var entities = _toMove.Resolve(context);
        var origin = _origin.Evaluate(context);

        long totalWeight = entities.Entities.Sum(e => e.Weight);
        long perTickCost = (long)Math.Ceiling(_perTickDistance * totalWeight);

        var drawn = context.DrawPower(perTickCost);
        if (drawn < perTickCost)
        {
            eventTracker.Add(new EffectNotFiredEvent(_effectName, $"Insufficient power: needed {perTickCost}, drew {drawn}"));
            return new(Advanced: false, EntitiesUnderMotion: []);
        }

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

        if (_remainingTicks == 0)
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

        return new(Advanced: true, EntitiesUnderMotion: entities.Entities);
    }
}
