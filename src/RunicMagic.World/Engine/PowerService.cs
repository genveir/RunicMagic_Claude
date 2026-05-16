using RunicMagic.World.Entities;
using RunicMagic.World.Execution;

namespace RunicMagic.World.Engine;

public class PowerService
{
    private readonly DamageService damageService;
    private readonly EntitySetSelectService entitySetSelectService;

    public PowerService(DamageService damageService, EntitySetSelectService entitySetSelectService)
    {
        this.damageService = damageService;
        this.entitySetSelectService = entitySetSelectService;
    }

    public long DrawPower(EntitySet entitySet, long amount, IWorldEventTracker eventTracker)
    {
        var groups = entitySet.Entities
            .Where(e => e.Reservoir != null)
            .GroupBy(e => e.Reservoir!.Current())
            .OrderByDescending(g => g.Key)
            .ToArray();

        long powerToDraw = amount;
        for (int n = 0; n < groups.Length; n++)
        {
            var group = groups[n];
            var drawn = DrawPower(group, powerToDraw, eventTracker);
            powerToDraw -= drawn;
            if (powerToDraw <= 0)
            {
                break;
            }
        }

        return amount - powerToDraw;
    }

    private long DrawPower(IEnumerable<Entity> entities, long amount, IWorldEventTracker eventTracker)
    {
        var perEntity = (long)Math.Ceiling((double)amount / entities.Count());
        var totalDrawn = 0L;
        foreach (var entity in entities)
        {
            var draw = entity.Reservoir!.Draw(perEntity);
            totalDrawn += draw.Amount;
            if (draw.Amount > 0)
            {
                eventTracker.Add(new PowerDrawnEvent(entity, draw.Amount));
                eventTracker.Track(entity);
            }
            if (draw.IsDrained)
            {
                eventTracker.Add(new EntityDrainedEvent(entity));
            }
        }
        return totalDrawn;
    }

    public void FillWithOvercharge(EntitySet toFill, EntitySet? returnSource, EntitySet caster, EntitySet executor, long amount, IWorldEventTracker eventTracker)
    {
        var remaining = amount - FillPower(toFill, amount, eventTracker);
        var currentSet = toFill;

        while (remaining > 0)
        {
            var scopeSelection = entitySetSelectService.GetUnionScope(currentSet.Entities);

            var scope = new EntitySet(scopeSelection.Entities.Where(e => e.Reservoir != null).ToList());

            var damageDealt = damageService.Damage(currentSet, remaining * 2, eventTracker);
            remaining -= (damageDealt + 1) / 2;

            if (remaining <= 0 || !scope.Entities.Any())
            {
                break;
            }

            remaining -= FillPower(scope, remaining, eventTracker);
            currentSet = scope;
        }

        if (remaining > 0)
        {
            var damageDealt = damageService.Damage(executor, remaining * 2, eventTracker);
            remaining -= (damageDealt + 1) / 2;
        }

        if (remaining > 0)
        {
            var damageDealt = damageService.Damage(caster, remaining * 2, eventTracker);
            remaining -= (damageDealt + 1) / 2;
        }

        if (remaining > 0 && returnSource != null)
        {
            FillWithOvercharge(
                toFill: returnSource,
                returnSource: null,
                caster: caster,
                executor: executor,
                amount: remaining,
                eventTracker: eventTracker);
        }
    }

    public long FillPower(EntitySet entitySet, long amount, IWorldEventTracker result)
    {
        var groups = entitySet.Entities
            .Where(e => e.Reservoir != null)
            .GroupBy(e => e.Reservoir!.Current())
            .OrderBy(g => g.Key)
            .ToArray();

        long powerToFill = amount;
        for (int n = 0; n < groups.Length; n++)
        {
            var group = groups[n];
            var filled = FillPower(group, powerToFill, result);
            powerToFill -= filled;
            if (powerToFill <= 0)
            {
                break;
            }
        }

        return amount - powerToFill;
    }

    private long FillPower(IEnumerable<Entity> entities, long amount, IWorldEventTracker result)
    {
        var perEntity = (long)Math.Ceiling((double)amount / entities.Count());
        var totalFilled = 0L;
        foreach (var entity in entities)
        {
            var fill = entity.Reservoir!.Fill(perEntity);
            totalFilled += fill.Amount;
            if (fill.Amount > 0)
            {
                result.Add(new PowerFilledEvent(entity, fill.Amount));
            }
            if (fill.IsFull)
            {
                result.Add(new EntityFullEvent(entity));
            }
        }
        return totalFilled;
    }
}
