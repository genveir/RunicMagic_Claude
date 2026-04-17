namespace RunicMagic.World.Execution;

public record SpellContext(EntitySet Caster, EntitySet Executor, WorldModel World, SpellResult Result)
{
    public int DrawPower(int amount)
    {
        var remaining = amount;
        foreach (var source in new[] { Executor, Caster })
        {
            if (remaining == 0)
            {
                break;
            }
            remaining -= source.DrawPower(remaining, Result);
        }
        return amount - remaining;
    }
}
