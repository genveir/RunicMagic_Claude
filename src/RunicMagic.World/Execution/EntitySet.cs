namespace RunicMagic.World.Execution;

public class EntitySet
{
    private readonly IReadOnlyList<Entity> _entities;

    public EntitySet(IReadOnlyList<Entity> entities)
    {
        _entities = entities;
    }

    public IReadOnlyList<Entity> Entities
    {
        get
        {
            return _entities;
        }
    }

    public EntitySet GetScope()
    {
        var seen = new HashSet<EntityId>();
        var scopeEntities = new List<Entity>();
        foreach (var entity in _entities)
        {
            var scope = entity.Scope?.Invoke() ?? [];
            foreach (var member in scope)
            {
                if (seen.Add(member.Id))
                {
                    scopeEntities.Add(member);
                }
            }
        }
        return new EntitySet(scopeEntities);
    }

    public long DrawPower(long amount, SpellResult result)
    {
        if (amount == 0)
        {
            return 0;
        }
        var sources = _entities.Where(e => e.Reservoir != null).ToList();
        if (sources.Count == 0)
        {
            return 0;
        }
        var perEntity = (long)Math.Ceiling((double)amount / sources.Count);
        var totalDrawn = 0L;
        foreach (var entity in sources)
        {
            var draw = entity.Reservoir!(perEntity);
            totalDrawn += draw.Amount;
            if (draw.Amount > 0)
            {
                result.Add(new PowerDrawnEvent(entity, draw.Amount));
            }
            if (draw.IsDrained)
            {
                result.Add(new EntityDrainedEvent(entity));
            }
        }
        return totalDrawn;
    }
}
