using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Motion.Shared;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Motion.Engine;

public class RotationEngineMotionEffect : IEngineMotionEffect
{
    private readonly TemporalSpellContext temporalContext;
    private readonly IEntitySet entities;
    private readonly ILocation origin;
    private readonly double perTickTheta;
    private readonly long totalRuneDegrees;
    private readonly string effectName;
    private int remainingTicks;

    public bool IsComplete => remainingTicks == 0;

    public RotationEngineMotionEffect(
        SpellContext context,
        IEntitySet toMove,
        ILocation origin,
        long totalRuneDegrees,
        int tickCount,
        bool isClockwise,
        string effectName)
    {
        temporalContext = new(context);
        entities = toMove;
        this.origin = origin;
        this.totalRuneDegrees = totalRuneDegrees;
        this.effectName = effectName;
        var sign = isClockwise ? 1.0 : -1.0;
        var totalTheta = sign * totalRuneDegrees / 2744.0 * 2 * Math.PI;
        perTickTheta = totalTheta / tickCount;
        remainingTicks = tickCount;
    }

    public EngineMotionEffectResult TryAdvance(IWorldEventTracker eventTracker)
    {
        var context = temporalContext.ToSpellContext(eventTracker);

        var entities = this.entities.Resolve(context);
        var origin = this.origin.Evaluate(context);

        var totalCost = 0L;
        foreach (var entity in entities.Entities)
        {
            totalCost += RotationCostCalculator.ComputeEntityCost(entity, origin, Math.Abs(perTickTheta));
        }

        var drawn = context.DrawPower(totalCost);
        if (drawn < totalCost)
        {
            eventTracker.Add(new EffectNotFiredEvent(effectName, $"Insufficient power: needed {totalCost}, drew {drawn}"));
            return new(Advanced: false, EntitiesUnderMotion: []);
        }

        var cos = Math.Cos(perTickTheta);
        var sin = Math.Sin(perTickTheta);

        foreach (var entity in entities.Entities)
        {
            var dx = entity.Location.X - origin.X;
            var dy = entity.Location.Y - origin.Y;
            var newLocation = new Location(
                origin.X + dx * cos - dy * sin,
                origin.Y + dx * sin + dy * cos
            );
            var newAngle = entity.FacingAngle + perTickTheta;
            MoveEntityService.Move(entity, newLocation, newAngle, eventTracker);
        }

        remainingTicks--;

        if (remainingTicks == 0)
        {
            foreach (var entity in entities.Entities)
            {
                eventTracker.Add(new EntityRotatedEvent(entity, totalRuneDegrees));
            }
        }

        return new(Advanced: true, EntitiesUnderMotion: entities.Entities);
    }
}
