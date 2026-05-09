using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Motion.Shared;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Motion.Engine;

public class LinearEngineMotionEffect : IEngineMotionEffect
{
    private readonly TemporalSpellContext temporalContext;
    private readonly IEntitySet toMove;
    private readonly ILocation origin;
    private readonly double perTickDistance;
    private readonly long totalDistanceMm;
    private readonly bool isAway;
    private readonly string effectName;
    private int remainingTicks;

    public bool IsComplete => remainingTicks == 0;

    public LinearEngineMotionEffect(
        SpellContext context,
        IEntitySet toMove,
        ILocation origin,
        double perTickDistance,
        long totalDistanceMm,
        bool isAway,
        string effectName)
    {
        temporalContext = new(context);
        this.toMove = toMove;
        this.origin = origin;
        this.perTickDistance = perTickDistance;
        this.totalDistanceMm = totalDistanceMm;
        this.isAway = isAway;
        this.effectName = effectName;
        remainingTicks = 56;
    }

    public EngineMotionEffectResult TryAdvance(IWorldEventTracker eventTracker)
    {
        var context = temporalContext.ToSpellContext(eventTracker);

        var entities = toMove.Resolve(context);
        var origin = this.origin.Evaluate(context);

        long totalWeight = entities.Entities.Sum(e => e.Weight);
        long perTickCost = (long)Math.Ceiling(perTickDistance * totalWeight);

        var drawn = context.DrawPower(perTickCost);
        if (drawn < perTickCost)
        {
            eventTracker.Add(new EffectNotFiredEvent(effectName, $"Insufficient power: needed {perTickCost}, drew {drawn}"));
            return new(Advanced: false, EntitiesUnderMotion: []);
        }

        foreach (var entity in entities.Entities)
        {
            Direction direction;
            if (isAway)
            {
                direction = Direction.FromPoints(origin, entity.Location);
            }
            else
            {
                direction = Direction.FromPoints(entity.Location, origin);
            }

            var destination = entity.Location.Translate(direction, perTickDistance);
            MoveEntityService.Move(entity, destination, entity.FacingAngle, eventTracker);
        }

        remainingTicks--;

        if (remainingTicks == 0)
        {
            foreach (var entity in entities.Entities)
            {
                if (isAway)
                {
                    eventTracker.Add(new EntityPushedEvent(entity, totalDistanceMm));
                }
                else
                {
                    eventTracker.Add(new EntityPulledEvent(entity, totalDistanceMm));
                }
            }
        }

        return new(Advanced: true, EntitiesUnderMotion: entities.Entities);
    }
}
