using RunicMagic.World.Entities;
using RunicMagic.World.Entities.Services;

namespace RunicMagic.World.Execution;

internal static class PowerService
{
    public static long DrawPower(EntitySet entitySet, long amount, IWorldEventTracker result)
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
            var drawn = DrawPower(group, powerToDraw, result);
            powerToDraw -= drawn;
            if (powerToDraw <= 0)
            {
                break;
            }
        }

        return amount - powerToDraw;
    }

    private static long DrawPower(IEnumerable<Entity> entities, long amount, IWorldEventTracker result)
    {
        var perEntity = (long)Math.Ceiling((double)amount / entities.Count());
        var totalDrawn = 0L;
        foreach (var entity in entities)
        {
            var draw = entity.Reservoir!.Draw(perEntity);
            totalDrawn += draw.Amount;
            if (draw.Amount > 0)
            {
                result.Add(new PowerDrawnEvent(entity, draw.Amount));
                result.Track(entity);
            }
            if (draw.IsDrained)
            {
                result.Add(new EntityDrainedEvent(entity));
            }
        }
        return totalDrawn;
    }

    public static void FillWithOvercharge(EntitySet toFill, EntitySet? returnSource, long amount, SpellContext context)
    {
        var remaining = amount - FillPower(toFill, amount, context.EventTracker);
        var currentSet = toFill;

        while (remaining > 0)
        {
            var scope = new EntitySet(currentSet.GetScope().Entities.Where(e => e.Reservoir != null).ToList());

            var damageDealt = DamageService.Damage(currentSet, remaining * 2, context);
            remaining -= (damageDealt + 1) / 2;

            if (remaining <= 0 || !scope.Entities.Any())
            {
                break;
            }

            remaining -= FillPower(scope, remaining, context.EventTracker);
            currentSet = scope;
        }

        if (remaining > 0)
        {
            var damageDealt = DamageService.Damage(context.Executor, remaining * 2, context);
            remaining -= (damageDealt + 1) / 2;
        }

        if (remaining > 0)
        {
            var damageDealt = DamageService.Damage(context.Caster, remaining * 2, context);
            remaining -= (damageDealt + 1) / 2;
        }

        if (remaining > 0 && returnSource != null)
        {
            FillWithOvercharge(returnSource, null, remaining, context);
        }
    }

    public static long FillPower(EntitySet entitySet, long amount, IWorldEventTracker result)
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

    private static long FillPower(IEnumerable<Entity> entities, long amount, IWorldEventTracker result)
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
